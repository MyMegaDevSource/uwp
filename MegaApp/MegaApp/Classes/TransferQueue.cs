﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MegaApp.Enums;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class TransferQueue
    {
        public TransferQueue()
        {
            this.QueuePaused = true;

            this.Uploads = new ObservableCollection<TransferObjectModel>();
            this.Downloads = new ObservableCollection<TransferObjectModel>();

            this.Uploads.CollectionChanged += OnCollectionChanged;
            this.Downloads.CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        /// Add a transfer to the Transfer Queue.
        /// </summary>
        /// <param name="transferObjectModel">Transfer to add</param>
        public void Add(TransferObjectModel transferObjectModel)
        {
            switch (transferObjectModel.Type)
            {
                case TransferType.Download:
                    Sort(this.Downloads, transferObjectModel);
                    break;
                case TransferType.Upload:
                    Sort(this.Uploads, transferObjectModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Select and return all transfers in the queue.
        /// </summary>
        /// <returns>Download and upload transfers combined in one list.</returns>
        public IList<TransferObjectModel> SelectAll()
        {
            var result = new List<TransferObjectModel>(this.Downloads.Count + this.Uploads.Count);
            result.AddRange(this.Downloads);
            result.AddRange(this.Uploads);
            return result;
        }

        /// <summary>
        /// Clear the complete queue
        /// </summary>
        public void Clear()
        {
            this.Downloads.Clear();
            this.Uploads.Clear();
        }
        
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        ((TransferObjectModel)item).PropertyChanged += OnStatusPropertyChanged;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        ((TransferObjectModel)item).PropertyChanged -= OnStatusPropertyChanged;
                    break;
            }
        }

        private void OnStatusPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Status")) return;

            var transferObjectModel = sender as TransferObjectModel;
            if (transferObjectModel == null) return;
            Sort(transferObjectModel.Type == TransferType.Download ? this.Downloads : this.Uploads, transferObjectModel);
        }

        public static void Sort(ObservableCollection<TransferObjectModel> transferList, 
            TransferObjectModel transferObject)
        {
            bool handled = false;
            var existing = transferList.FirstOrDefault(
                t => t.TransferPath.Equals(transferObject.TransferPath));
            bool move = existing != null;
            var index = transferList.IndexOf(existing);
            var count = transferList.Count - 1;

            for (var i = 0; i <= count; i++)
            {
                if ((int)transferObject.Status > (int)transferList[i].Status) continue;

                if (move)
                {
                    if (index != i)
                        transferList.Move(index, i);
                }
                else
                {
                    transferList.Insert(i, transferObject);
                }
                handled = true;
                break;
            }

            if (handled) return;

            if (move)
            {
                if (index != count)
                    transferList.Move(index, count);
            }
            else
            {
                transferList.Add(transferObject);
            }   
        }

        public ObservableCollection<TransferObjectModel> Uploads { get; }

        public ObservableCollection<TransferObjectModel> Downloads { get; }

        public bool QueuePaused { get; set; }
    }
}

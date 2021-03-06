﻿using System;
using System.Threading.Tasks;

namespace BackgroundTaskService.Services
{
    internal static class ErrorHandlingService
    {
        public const string ImageErrorFileSetting = "ImageErrorFileName";
        public const string ImageErrorCountSetting = "ImageErrorCount";
        public const int MaxFileUploadErrors = 10;


        /// <summary>
        /// Save the filename and error count of that specific file
        /// </summary>
        /// <param name="fileName">The file that failed</param>
        /// <param name="fileSetting">File to save the name of the file that failed</param>
        /// <param name="countSetting">File to save the error count of the file that failed</param>
        /// <returns>Number of current errors</returns>
        public static async Task<int> SetFileErrorAsync(string fileName, string fileSetting, string countSetting)
        {
            try
            {
                // Load filename last error
                var lastErrorFileName = await SettingsService.LoadSettingFromFileAsync<string>(fileSetting);

                // Check if it is the same file that has an error again
                if (!string.IsNullOrEmpty(lastErrorFileName) &&
                    lastErrorFileName.Equals(fileName))
                {
                    // If the same file, add to the error count, save and return
                    var count = await SettingsService.LoadSettingFromFileAsync<int>(countSetting);
                    count++;
                    await SettingsService.SaveSettingToFileAsync(countSetting, count);
                    return count;
                }

                // New file error. Save the name and set the error count to one.
                await SettingsService.SaveSettingToFileAsync(fileSetting, fileName);
                await SettingsService.SaveSettingToFileAsync(countSetting, 1);
                return 1;
            }
            catch (Exception)
            {
                // Do not let the error process cause the main service to generate a fault
                return 0;
            }
        }

        /// <summary>
        /// Check if need to skip the file because of to many errors
        /// </summary>
        /// <param name="fileName">The file to check for errors</param>
        /// <param name="fileSetting">File to load the name of the file that failed</param>
        /// <param name="countSetting">File to load the error count of the file that failed</param>
        /// <returns></returns>
        public static async Task<bool> SkipFileAsync(string fileName, string fileSetting, string countSetting)
        {
            var lastErrorFileName = await SettingsService.LoadSettingFromFileAsync<string>(fileSetting);
            if (string.IsNullOrEmpty(lastErrorFileName) || !lastErrorFileName.Equals(fileName)) return false;

            var count = await SettingsService.LoadSettingFromFileAsync<int>(countSetting);
            return count >= (MaxFileUploadErrors - 1);
        }

        /// <summary>
        /// Clear filename and error count for error processing
        /// </summary>
        /// <param name="fileSetting">File to clear the name</param>
        /// <param name="countSetting">File to clear the error count of the file that failed</param>
        public static async Task ClearAsync(string fileSetting, string countSetting)
        {
            await SettingsService.SaveSettingToFileAsync(fileSetting, string.Empty);
            await SettingsService.SaveSettingToFileAsync(countSetting, 0);
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618

namespace JackFrame {

    public static class UnityFileHelper {

        public async static Task<string> ReadFileFromStreamingAssetsAsync(string filePath) {
            string path = ConvertPath(filePath);
            try {
                UnityWebRequest request = UnityWebRequest.Get(path);
                request.SendWebRequest();//读取数据
                while (!request.downloadHandler.isDone) {
                    await Task.Delay(1);
                }
                return request.downloadHandler.text;
            }
            catch (Exception ex) {
                PLog.Error("获取文件失败" + ex);
            }
            return null;
        }

        public async static Task ReadFileFromStreamingAssetsAsync(string filePath, Action<byte[]> action) {
            string path = ConvertPath(filePath);
            try {
                UnityWebRequest request = UnityWebRequest.Get(path);
                request.SendWebRequest();//读取数据
                while (!request.downloadHandler.isDone) {
                    await Task.Delay(1);
                }
                action(request.downloadHandler.data);
            }
            catch (Exception ex) {
                PLog.Error("获取文件失败" + ex);
            }
            //readFileTask.Start();
        }

        /// <summary>
        /// 根据需要将路径转换不同平台的路径
        /// </summary>
        private static string ConvertPath(string filePath) {
            string path =
#if UNITY_ANDROID && !UNITY_EDITOR
            Path.Combine(Application.streamingAssetsPath, filePath); //安卓的Application.streamingAssetsPath已默认有"file://"
#elif UNITY_IOS && !UNITY_EDITOR
            Path.Combine("file://", Application.streamingAssetsPath, filePath);
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
            Path.Combine("file://", Application.streamingAssetsPath, filePath);
#else
		string.Empty;
#endif
            return path;
        }

    }
}
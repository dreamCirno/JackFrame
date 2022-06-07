using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace JackFrame {

    public static class AddressablesHelper {

        public static async Task LoadWithLabel<T>(string label, Action<T> callback) {
            try {
                var reference = new AssetLabelReference();
                reference.labelString = label;
                var list = await Addressables.LoadAssetsAsync<T>(reference, null).Task;
                for (int i = 0; i < list.Count; i += 1) {
                    T t = list[i];
                    callback(t);
                }
            } catch (Exception e) {
                throw e;
            }

        }

    }
}
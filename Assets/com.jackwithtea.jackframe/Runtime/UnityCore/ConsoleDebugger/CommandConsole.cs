using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace JackFrame {

    public class CommandConsole {

        Dictionary<string, Action<string, string, string, string>> actionDic = new Dictionary<string, Action<string, string, string, string>>();
        Dictionary<string, string> descriptionDic = new Dictionary<string, string>();
        Dictionary<string, Action> oneActionDic = new Dictionary<string, Action>();
        public void RegisterCommand(Action handle, string command, string description) {
            command = command.ToLower();

            actionDic.Add(command, (s1, s2, s3, s4) => handle());
            descriptionDic.Add(command, description);

        }

        public void RegisterCommand(Action handle, string command, string description, bool isUnityAction) {
            command = command.ToLower();
            actionDic.Add(command, (s1, s2, s3, s4) => handle());
            oneActionDic.Add(command, handle);
            descriptionDic.Add(command, description);

        }

        public void RegisterCommand<T>(Action<T> handle, string command, string description) {
            command = command.ToLower();

            actionDic.Add(command, (s1, s2, s3, s4) => handle(s1.Parse<T>()));
            descriptionDic.Add(command, description);
        }

        public void RegisterCommand<T1, T2>(Action<T1, T2> handle, string command, string description) {
            command = command.ToLower();
            actionDic.Add(command, (s1, s2, s3, s4) => handle(s1.Parse<T1>(), s2.Parse<T2>()));
            descriptionDic.Add(command, description);
        }

        public void RegisterCommand<T1, T2, T3>(Action<T1, T2, T3> handle, string command, string description) {
            command = command.ToLower();
            actionDic.Add(command, (s1, s2, s3, s4) => handle(s1.Parse<T1>(), s2.Parse<T2>(), s3.Parse<T3>()));
            descriptionDic.Add(command, description);
        }

        public void RegisterCommand<T1, T2, T3, T4>(Action<T1, T2, T3, T4> handle, string command, string description) {
            command = command.ToLower();
            actionDic.Add(command, (s1, s2, s3, s4) => handle(s1.Parse<T1>(), s2.Parse<T2>(), s3.Parse<T3>(), s4.Parse<T4>()));
            descriptionDic.Add(command, description);
        }

        public List<(string command, string description)> ShowTips(string currentCommand) {
            currentCommand = currentCommand.ToLower();
            
            string cmd = currentCommand.Split(' ')[0];
            List<(string, string)> list = new List<(string, string)>();
            List<string> commandList = actionDic.GetKeyList();
            commandList = commandList.FindAll(value => value.Contains(cmd));
            commandList.ForEach(value => {
                list.Add((value, descriptionDic[value]));
            });
            return list;
        }

        public Dictionary<string, Action> GetEvent(string currentCommand) {
            currentCommand = currentCommand.ToLower();
            string cmd = currentCommand.Split(' ')[0];
            List<Action> list = new List<Action>();
            Dictionary<string, Action> ActionDic = oneActionDic;

            return ActionDic;
        }

        public void Trigger(string fullCommand) {

            if (string.IsNullOrEmpty(fullCommand)) {
                return;
            }

            string[] arr = fullCommand.Split(' ');

            string command = arr[0];
            Action<string, string, string, string> action = actionDic.GetValue(command);
            if (action == null) {
                return;
            }

            string[] paramArr = new string[]{"", "", "", ""};
            for (int i = 1; i < arr.Length; i += 1) {
                paramArr[i - 1] = arr[i];
            }

            action.Invoke(paramArr[0], paramArr[1], paramArr[2], paramArr[3]);

        }

    }

}
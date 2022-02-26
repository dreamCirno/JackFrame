using System;
using UnityEngine;

namespace JackFrame {

    public class CursorSetting {

        public void LockCursor() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor() {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public void SwitchLockState() {
            if (Cursor.lockState == CursorLockMode.Locked) {
                UnlockCursor();
            } else {
                LockCursor();
            }
        }
    }
}
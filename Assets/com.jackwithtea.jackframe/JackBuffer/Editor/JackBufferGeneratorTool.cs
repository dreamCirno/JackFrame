using UnityEngine;
using JackBuffer;

namespace JackBuffer.Editor {

    public class JackBufferGeneratorTool {

        [UnityEditor.MenuItem("JackFrame/JackBuffer/Gen")]
        public static void Gen() {

            JackBufferGenerator.GenModel(Application.dataPath + "/com.jackwithtea.jackframe/JackBuffer/Samples", "");

        }

    }

}
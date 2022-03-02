using UnityEngine;
using JackBuffer;

namespace JackBuffer.Editor {

    public class JackBufferGeneratorSampleTool {

        // [UnityEditor.MenuItem("JackFrame/JackBuffer/Sample/Gen")]
        public static void Gen() {

            JackBufferGenerator.GenModel(Application.dataPath + "/com.jackwithtea.jackframe/JackBuffer/Samples/Messages");

        }

    }

}
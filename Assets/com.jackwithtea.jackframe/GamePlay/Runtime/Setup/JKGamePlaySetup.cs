using System;

namespace JackGamePlay {

    public static class JKGamePlaySetup {

        // 度量单位
        public static float MeasurementUnit { get; private set; } = 1f;

        public static void SetMeasurementUnit(float measurementUnit) {
            MeasurementUnit = measurementUnit;
        }

    }

}
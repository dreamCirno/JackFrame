namespace BEPUPhysics1int {

    public enum FreezeRotationFlag : byte {
        None = 0,
        RotX = 1,
        RotY = 2,
        RotZ = 4,
        AllRot = 7, 
        PosX = 8,
        PosY = 16,
        PosZ = 32,
        AllPos = 56
    }

}
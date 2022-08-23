using System;
using UnityEngine;
public enum EConveyorType
{
    MIDDLE,
}
public enum EConveyorImg
{
    DEFAULT,
    PLATE_CLOCKWISE,
    PLATE_COUNTERCLOCKWISE,
}
[System.Serializable]
public class ConveyorInfo
{
    [SerializeField] string Type;
    [SerializeField] string In;
    [SerializeField] string Out;
    [SerializeField] string ImageType;
    [SerializeField] int PortalIndex;

    ////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////
    public ConveyorInfo()
    {
        Type = EConveyorType.MIDDLE.ToString();
        In = EScrollDir.LEFT.ToString();
        Out = EScrollDir.RIGHT.ToString();
        ImageType = EConveyorImg.DEFAULT.ToString();
        PortalIndex = 0;
    }

    public ConveyorInfo GetClone()
    {
        return MemberwiseClone() as ConveyorInfo;
    }

    public void SetConveyorType(EConveyorType type)
    {
        Type = type.ToString();
    }
    public EConveyorType GetConveyorType()
    {
        EConveyorType type = EConveyorType.MIDDLE;
        bool ok = Enum.TryParse(Type, true, out type);
        return type;
    }

    public void SetDirIn(EScrollDir dir)
    {
        In = dir.ToString();
    }
    public EScrollDir GetDirIn()
    {
        EScrollDir dir = EScrollDir.LEFT;
        Enum.TryParse(In, true, out dir);
        return dir;
    }

    public void SetDirOut(EScrollDir dir)
    {
        Out = dir.ToString();
    }
    public EScrollDir GetDirOut()
    {
        EScrollDir dir = EScrollDir.LEFT;
        Enum.TryParse(Out, true, out dir);
        return dir;
    }

    public void SetConveyorImg(EConveyorImg image)
    {
        ImageType = image.ToString();
    }
    public EConveyorImg GetConveyorImg()
    {
        EConveyorImg img = EConveyorImg.DEFAULT;
        Enum.TryParse(ImageType, true, out img);
        return img;
    }

    public bool CheckOutDirValid(EScrollDir outDir)
    {
        EScrollDir inDir = GetDirIn();
        EConveyorImg imgType = GetConveyorImg();

        if (imgType == EConveyorImg.PLATE_CLOCKWISE)
        {
            if (inDir == EScrollDir.LEFT && outDir == EScrollDir.DOWN) return true;
            if (inDir == EScrollDir.DOWN && outDir == EScrollDir.RIGHT) return true;
            if (inDir == EScrollDir.RIGHT && outDir == EScrollDir.UP) return true;
            if (inDir == EScrollDir.UP && outDir == EScrollDir.LEFT) return true;
        }
        else if (imgType == EConveyorImg.PLATE_COUNTERCLOCKWISE)
        {
            if (inDir == EScrollDir.DOWN && outDir == EScrollDir.LEFT) return true;
            if (inDir == EScrollDir.RIGHT && outDir == EScrollDir.DOWN) return true;
            if (inDir == EScrollDir.UP && outDir == EScrollDir.RIGHT) return true;
            if (inDir == EScrollDir.LEFT && outDir == EScrollDir.UP) return true;
        }
        else if (imgType == EConveyorImg.DEFAULT)
        {
            if (inDir != outDir) return true;
        }
        return false;
    }
}
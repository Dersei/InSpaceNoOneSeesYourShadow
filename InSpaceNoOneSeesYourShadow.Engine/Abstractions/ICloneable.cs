namespace InSpaceNoOneSeesYourShadow.Engine.Abstractions
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
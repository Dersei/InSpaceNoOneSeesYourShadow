namespace InSpaceNoOneSeesYourShadow.Engine.Abstractions
{
    internal class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T GetInstance()
        {
            return _instance ??= new T();
        }
    }
}

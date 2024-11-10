using WatchMe.Repository.Implementations;

namespace WatchMe.Repository
{
    public interface IVideoRepositoryFactory
    {
        public IVideoRepository GetVideoRepository();
    }
    public class VideoRepositoryFactory : IVideoRepositoryFactory
    {
        public IVideoRepository GetVideoRepository()
        {
#if ANDROID
            return new AndroidVideoRepository();
#endif
            throw new NotImplementedException();
        }
    }
}

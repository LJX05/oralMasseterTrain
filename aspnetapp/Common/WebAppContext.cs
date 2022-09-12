namespace aspnetapp.Common
{
    public class WebAppContext
    {
        private WebAppContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public static void Init(IServiceProvider serviceProvider)
        {
            if (_Instance != null)
            {
                return;
            }
            _Instance = new WebAppContext(serviceProvider);  
        }
        private static WebAppContext _Instance;
        public static WebAppContext Instance {
            get
            {
                return _Instance;
            }
        }
        private IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider
        {
            get
            {
                return _serviceProvider;
            }
        }

        public HttpRequest CurrentRequest
        {
            get;set;
        }

    }



}

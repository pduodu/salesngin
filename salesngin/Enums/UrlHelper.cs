namespace salesngin.Enums
{
    public class UrlHelper
    {
        private IHttpContextAccessor _httpContextAccessor;

        public UrlHelper()
        {

        }

        public UrlHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetBaseUrl()
        {
            if (_httpContextAccessor.HttpContext.Request != null)
            {
                var request = _httpContextAccessor.HttpContext.Request;

                var host = request.Host.ToUriComponent();

                var pathBase = request.PathBase.ToUriComponent();

                return $"{request.Scheme}://{host}{pathBase}";
            }
            else
            {
                return "~";
            }
        }
    }
}

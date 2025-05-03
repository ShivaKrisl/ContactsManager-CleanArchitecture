using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters.ResourceFilters
{
    public class FeatureDisableResourceFilter : IAsyncResourceFilter
    {

        private readonly ILogger<FeatureDisableResourceFilter> _logger;

        private readonly bool _disable;

        public FeatureDisableResourceFilter(ILogger<FeatureDisableResourceFilter> logger, bool disable=true)
        {
            _logger = logger;
            _disable = disable;
        }
        
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            // before 

            _logger.LogInformation("Start--------------{FilterName}.{Method}--------------", nameof(FeatureDisableResourceFilter), nameof(OnResourceExecutionAsync));

            if (_disable)// if that action method is disabled
            {
               context.Result = new StatusCodeResult(501); // Not implemented exception - terminate the entire request pipeline
            }
            else
            {
                await next();
            }

            // after 
            _logger.LogInformation("--------------{FilterName}.{Method}--------------End", nameof(FeatureDisableResourceFilter), nameof(OnResourceExecutionAsync));
        }
    }
}

using VContainer;
using VContainer.Unity;

namespace Scripts.Bootstrap
{
    public class MenuLifetimeScope : LifetimeScope
    {
        protected override void Awake()
        {
            EnqueueParent(FindFirstObjectByType<BootstrapLifetimeScope>());
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}

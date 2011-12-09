using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Diagnostics;
using StructureMap;
using StructureMap.Pipeline;

namespace OpenRasta.DI.StructureMap
{
	public class StructureMapDependencyResolver : DependencyResolverCore, IDependencyResolver
	{
		private static IContainer _container;
		private readonly object _locker = new object();

		public StructureMapDependencyResolver()
			: this(ObjectFactory.Container) {
		}

		public StructureMapDependencyResolver(IContainer container)
		{
			if (_container == null) {
				lock (_locker) {
					if (_container == null) {
						_container = container;
						_container.Configure(ex => ex.FillAllPropertiesOfType<ILogger>());
					}
				}
			}
		}

		protected override void AddDependencyCore(Type serviceType, Type concreteType, DependencyLifetime lifetime)
		{
			_container.Configure(cfg => cfg.For(serviceType).LifecycleIs(GetLifecycle(lifetime)).Use(concreteType));
		}

		private static InstanceScope GetLifecycle(DependencyLifetime lifetime)
		{
			switch (lifetime)
			{
				case DependencyLifetime.PerRequest:
					return InstanceScope.HttpContext;
				case DependencyLifetime.Singleton:
					return InstanceScope.Singleton;
				case DependencyLifetime.Transient:
					return InstanceScope.Transient;
				default:
					return InstanceScope.HttpContext;
			}
		}

		protected override void AddDependencyCore(Type concreteType, DependencyLifetime lifetime)
		{
			_container.Configure(cfg => cfg.For(concreteType).LifecycleIs(GetLifecycle(lifetime)).Use(concreteType));
		}

		protected override void AddDependencyInstanceCore(Type serviceType, object instance, DependencyLifetime lifetime)
		{
			lock (_locker)
			{
				_container.Configure(cfg => cfg.For(serviceType).LifecycleIs(GetLifecycle(lifetime)).Use(instance).Named(serviceType.FullName));
				ResolveCore(serviceType);
			}
		}

		protected override IEnumerable<TService> ResolveAllCore<TService>()
		{
			lock (_locker)
			{
				return _container.GetAllInstances<TService>();
			}
		}

		protected override object ResolveCore(Type serviceType)
		{
			lock (_locker)
			{
				return _container.GetInstance(serviceType);
			}
		}

		public bool HasDependency(Type serviceType)
		{
			lock (_locker)
			{
				return _container.TryGetInstance(serviceType) != null;
			}
		}

		public bool HasDependencyImplementation(Type serviceType, Type concreteType)
		{
			if (!_container.Model.HasImplementationsFor(serviceType))
				return false;

			return _container.Model.For(serviceType).Instances.Any(i => i.ConcreteType == concreteType);
		}

		public void HandleIncomingRequestProcessed()
		{
			HttpContextLifecycle.DisposeAndClearAll();
		}
	}
}
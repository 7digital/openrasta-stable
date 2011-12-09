using System;
using System.Threading;
using Castle.Windsor;
using NUnit.Framework;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Windsor.Tests.Unit {
	
	[TestFixture]
	public class ConcurrencyTests
	{
		private static WindsorDependencyResolver _resolver;

		[Test]
		public void Should_not_throw_exception_when_multiple_threads_manipulate_the_Dependency_resolver() {
			int errorCount = 0;

			for (int i = 0; i < 1000; i++ ) {
				new Thread(() => {
					var container = new WindsorContainer();
					_resolver = new WindsorDependencyResolver(container);
					_resolver.AddDependency(typeof(IDependencyResolver), typeof(WindsorDependencyResolver), DependencyLifetime.Singleton);
					_resolver.AddDependency(typeof(IWindsorContainer), typeof(WindsorContainer), DependencyLifetime.Singleton);

					_resolver.AddDependency(typeof(IPipeline), typeof(PipelineRunner), DependencyLifetime.Singleton);

					try {
						_resolver.Resolve<IPipeline>();
					} catch (Exception) {
						errorCount = errorCount + 1;
						throw;
					}

				}).Start();
			}

			Assert.That(errorCount, Is.EqualTo(0), "Some of the requests threw errors meaning that the dependencyresolver is not threadsafe ");
		}
	}
}

using System.Threading.Tasks;
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
			Parallel.For(0, 1000, 
				i=>{
					var container = new WindsorContainer();
					_resolver = new WindsorDependencyResolver(container);
					_resolver.AddDependency(typeof(IDependencyResolver), typeof(WindsorDependencyResolver), DependencyLifetime.Singleton);
					_resolver.AddDependency(typeof(IWindsorContainer), typeof(WindsorContainer), DependencyLifetime.Singleton);
					
					_resolver.AddDependency(typeof(IPipeline), typeof(PipelineRunner), DependencyLifetime.Singleton);
					
					var pipeline = _resolver.Resolve<IPipeline>();
					Assert.That(pipeline, Is.Not.Null);
				}
			);
		}

	}
}

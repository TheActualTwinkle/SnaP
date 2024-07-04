#if !NOT_UNITY3D

using System;
using System.Linq;
using System.Reflection;
using ModestTree;
using UnityEngine;
using UnityEngine.SceneManagement;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable RedundantArgumentDefaultValue

namespace Zenject
{
    /// <summary>
    /// An extension that allows locating Zenject context/container to which gameobject belongs to.
    /// As a side product, it's also allows to use Zenject framework in the <see cref="https://en.wikipedia.org/wiki/Service_locator_pattern">Service Locator Pattern</see> style.
    /// </summary>
    public static class ZenjectLocator
    {
        #region[ GetContainer ]

        /// <summary>
        /// Aliace for the ProjectContext container.
        /// </summary>
        /// <example>
        /// <code>
        /// public class Foo
        /// {
        ///     public void DoSomething()
        ///     {
        ///        ZenjectLocator.Container.Resolve{ISomeService}.PerformTask();
        ///        ...
        ///     }
        /// }
        /// </code>
        /// </example>
        public static DiContainer Container
        {
            get
            {
                var context = Zenject.ProjectContext.Instance;
                return (context != null) ? context.Container : null;
            }
        }

        /// <summary>
        /// Returns the most 'local' container of any context type to the target scene, game object or component.
        /// See more about 'local' context/container conception in <see cref="ZenjectBinding._context"/>.
        /// </summary>
        /// <param name="scene/gameObject/component">Scene, game object or component for which container should be found.</param>
        /// <param name="create" default="true">Automatically creates new container context in hierarchy of related scene if required container was not found.</param>
        /// <returns>Most 'local' container of any context type.</returns>
        /// <example>
        /// <code>
        /// public class Foo : MonoBehaviour
        /// {
        ///     private void OnCollisionEnter(Collision collision)
        ///     {
        ///        ZenjectLocator.GetContainer(collision.gameObject).Resolve{ISomeService}.PerformTask();
        ///        ...
        ///     }
        /// }
        /// </code>
        /// </example>
        public static DiContainer GetContainer(Scene scene)
        {
            return GetContainer(scene, true);
        }
        public static DiContainer GetContainer(Scene scene, bool create)
        {
            var context = GetContext(scene, create);
            return (context != null) ? context.Container : null;
        }
        public static DiContainer GetContainer(GameObject gameObject)
        {
            return GetContainer(gameObject, true);
        }
        public static DiContainer GetContainer(GameObject gameObject, bool create)
        {
            var context = GetContext(gameObject, create);
            return (context != null) ? context.Container : null;
        }
        public static DiContainer GetContainer(Component component)
        {
            return GetContainer(component, true);
        }
        public static DiContainer GetContainer(Component component, bool create)
        {
            var context = GetContext(component, create);
            return (context != null) ? context.Container : null;
        }

        /// <summary>
        /// Returns the most 'local' container of specified context type to the target scene, game object or component.
        /// See more about 'local' context/container conception in <see cref="ZenjectBinding._context"/>.
        /// </summary>
        /// <param name="scene/gameObject/component">Scene, game object or component for which container should be found.</param>
        /// <param name="type">Type (base type) of context that should be found.</param>
        /// <param name="create" default="true">Automatically creates new container context in hierarchy of related scene if required container was not found.</param>
        /// <returns>Most 'local' container of specified context type.</returns>
        /// <example>
        /// <code>
        /// public class Foo : MonoBehaviour
        /// {
        ///     private void OnCollisionEnter(Collision collision)
        ///     {
        ///        ZenjectLocator.GetContainer(collision.gameObject, typeof(GameObjectContext)).Resolve{ISomeService}.PerformTask();
        ///        ...
        ///     }
        /// }
        /// </code>
        /// </example>
        public static DiContainer GetContainer(Scene scene, Type type)
        {
            return GetContainer(scene, type, true);
        }
        public static DiContainer GetContainer(Scene scene, Type type, bool create)
        {
            var context = GetContext(scene, type, create);
            return (context != null) ? context.Container : null;
        }
        public static DiContainer GetContainer(GameObject gameObject, Type type)
        {
            return GetContainer(gameObject, type, true);
        }
        public static DiContainer GetContainer(GameObject gameObject, Type type, bool create)
        {
            var context = GetContext(gameObject, type, create);
            return (context != null) ? context.Container : null;
        }
        public static DiContainer GetContainer(Component component, Type type)
        {
            return GetContainer(component, type, true);
        }
        public static DiContainer GetContainer(Component component, Type type, bool create)
        {
            var context = GetContext(component, type, create);
            return (context != null) ? context.Container : null;
        }

        /// <summary>
        /// Returns the most 'local' container of specified context type to the target scene, game object or component.
        /// See more about 'local' context/container conception in <see cref="ZenjectBinding._context"/>.
        /// </summary>
        /// <typeparam name="T">Type (base type) of context that should be found.</typeparam>
        /// <param name="scene/gameObject/component">Scene, game object or component for which container should be found.</param>
        /// <param name="create" default="true">Automatically creates new container context in hierarchy of related scene if required container was not found.</param>
        /// <returns>Most 'local' container of specified context type.</returns>
        /// <example>
        /// <code>
        /// public class Foo : MonoBehaviour
        /// {
        ///     private void OnCollisionEnter(Collision collision)
        ///     {
        ///        ZenjectLocator.GetContainer{GameObjectContext}(collision.gameObject).Resolve{ISomeService}.PerformTask();
        ///        ...
        ///     }
        /// }
        /// </code>
        /// </example>
        public static DiContainer GetContainer<T>(Scene scene) where T : Context
        {
            return GetContainer(scene, true);
        }
        public static DiContainer GetContainer<T>(Scene scene, bool create) where T : Context
        {
            var context = GetContext<T>(scene, create);
            return (context != null) ? context.Container : null;
        }
        public static DiContainer GetContainer<T>(GameObject gameObject) where T : Context
        {
            return GetContainer(gameObject, true);
        }
        public static DiContainer GetContainer<T>(GameObject gameObject, bool create) where T : Context
        {
            var context = GetContext<T>(gameObject, create);
            return (context != null) ? context.Container : null;
        }
        public static DiContainer GetContainer<T>(Component component) where T : Context
        {
            return GetContainer<T>(component, true);
        }
        public static DiContainer GetContainer<T>(Component component, bool create) where T : Context
        {
            var context = GetContext<T>(component, create);
            return (context != null) ? context.Container : null;
        }

        #endregion

        #region[ GetContext ]

        // We do not want to patch Zenject framework sources so we will use reflection to get to the required functionality.
        static readonly FieldInfo _sceneContextStaticAutoRunBindingField = typeof(SceneContext).GetField("_staticAutoRun", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Aliace for the ProjectContext.
        /// </summary>
        public static Context ProjectContext
        {
            get { return Zenject.ProjectContext.Instance; }
        }

        /// <summary>
        /// Returns the most 'local' context of any type to the target scene, game object or component.
        /// See more about 'local' context/container conception in <see cref="ZenjectBinding._context"/>.
        /// </summary>
        /// <param name="scene/gameObject/component">Scene, game object or component for which context should be found.</param>
        /// <param name="create" default="true">Automatically creates new context in hierarchy of related scene if required container was not found.</param>
        /// <returns>Most 'local' context of any type.</returns>
        public static Context GetContext(Scene scene)
        {
            return GetContext(scene, true);
        }
        public static Context GetContext(Scene scene, bool create)
        {
            return GetContext<Context>(scene, create);
        }
        public static Context GetContext(GameObject gameObject)
        {
            return GetContext(gameObject, true);
        }
        public static Context GetContext(GameObject gameObject, bool create)
        {
            return GetContext<Context>(gameObject, create);
        }
        public static Context GetContext(Component component)
        {
            return GetContext(component, true);
        }
        public static Context GetContext(Component component, bool create)
        {
            return GetContext<Context>(component, create);
        }

        /// <summary>
        /// Returns the most 'local' context of specified type to the target scene, game object or component.
        /// See more about 'local' context/container conception in <see cref="ZenjectBinding._context"/>.
        /// </summary>
        /// <param name="scene/gameObject/component">Scene, game object or component for which context should be found.</param>
        /// <param name="type">Type (base type) of context that should be found.</param>
        /// <param name="create" default="true">Automatically creates new context in hierarchy of related scene if required container was not found.</param>
        /// <returns>Most 'local' context of specified type.</returns>
        public static Context GetContext(Scene scene, Type type)
        {
            return GetContext(scene, type, true);
        }
        public static Context GetContext(Scene scene, Type type, bool create)
        {
            Assert.IsNotNull(scene);
            Assert.DerivesFromOrEqual<Context>(type);

            if (type.DerivesFromOrEqual<ProjectContext>())
            {
                return Zenject.ProjectContext.Instance;
            }

            var contextType = (type == typeof (Context)) ? typeof (SceneContext) : type;
            var contextComponent = GameObject
                .FindObjectsOfType(contextType)
                .Cast<Component>()
                .FirstOrDefault(comp => comp.gameObject.scene == scene);

            if (create && contextComponent == null)
            {
                if (contextType == typeof (SceneContext))
                {
                    contextComponent = SceneContext.Create();
                }
                else if (contextType.DerivesFromOrEqual<SceneContext>())
                {
                    _sceneContextStaticAutoRunBindingField.SetValue(null, false);
                    contextComponent = new GameObject(typeof (SceneContext).Name).AddComponent(contextType);
                    Assert.That((bool)_sceneContextStaticAutoRunBindingField.GetValue(null)); // Should be reset
                }
                else
                {
                    var contextParentContainer = GetContainer(scene, typeof (SceneContext), true);
                    var contextGameObject = new GameObject(contextType.Name);
                    contextComponent = contextParentContainer.InstantiateComponent(contextType, contextGameObject, new object[0]);
                }

                if (contextComponent.gameObject.scene != scene)
                {
                    SceneManager.MoveGameObjectToScene(contextComponent.gameObject, scene);
                }
            }

            return (contextComponent as Context);
        }
        public static Context GetContext(GameObject gameObject, Type type)
        {
            return GetContext(gameObject, type, true);
        }
        public static Context GetContext(GameObject gameObject, Type type, bool create)
        {
            Assert.IsNotNull(gameObject);
            Assert.DerivesFromOrEqual<Context>(type);

            if (type.DerivesFromOrEqual<ProjectContext>())
            {
                return Zenject.ProjectContext.Instance;
            }
            if (type.DerivesFromOrEqual<SceneContext>())
            {
                return GetContext(gameObject.scene, type, true);
            }

            Component contextComponent = null;
            Type contextType = type;
            if (gameObject.activeInHierarchy)
            {
                contextComponent = gameObject.GetComponentInParent(contextType);
            }
            else
            {
                var transform = gameObject.transform;
                do
                {
                    contextComponent = transform.GetComponent(contextType);
                    transform = transform.parent;
                } while (contextComponent == null && transform != null);
            }

            if (create && contextComponent == null)
            {
                if (contextType == typeof (Context))
                {
                    contextComponent = GetContext(gameObject.scene, typeof (SceneContext), true);
                }
                else
                {
                    var contextParentContainer =
                        GetContainer(gameObject, typeof (Context), false) ??
                        GetContainer(gameObject.scene, typeof (SceneContext), true);
                    var contextGameObject = gameObject;
                    contextComponent = contextParentContainer.InstantiateComponent(contextType, contextGameObject, new object[0]);
                }
            }

            return (contextComponent as Context);
        }
        public static Context GetContext(Component component, Type type)
        {
            return GetContext(component, type, true);
        }
        public static Context GetContext(Component component, Type type, bool create)
        {
            Assert.IsNotNull(component);
            return GetContext(component.gameObject, type, create);
        }

        /// <summary>
        /// Returns the most 'local' context of specified type to the target scene, game object or component.
        /// See more about 'local' context/container conception in <see cref="ZenjectBinding._context"/>.
        /// </summary>
        /// <typeparam name="T">Type (base type) of context that should be found.</typeparam>
        /// <param name="scene/gameObject/component">Scene, game object or component for which context should be found.</param>
        /// <param name="create" default="true">Automatically creates new context in hierarchy of related scene if required container was not found.</param>
        /// <returns>Most 'local' context of specified type.</returns>
        public static T GetContext<T>(Scene scene) where T : Context
        {
            return GetContext<T>(scene, true);
        }
        public static T GetContext<T>(Scene scene, bool create) where T : Context
        {
            return (GetContext(scene, typeof(T), create) as T);
        }
        public static T GetContext<T>(GameObject gameObject) where T : Context
        {
            return GetContext<T>(gameObject, true);
        }
        public static T GetContext<T>(GameObject gameObject, bool create) where T : Context
        {
            return (GetContext(gameObject, typeof(T), create) as T);
        }
        public static T GetContext<T>(Component component) where T : Context
        {
            return GetContext<T>(component, true);
        }
        public static T GetContext<T>(Component component, bool create) where T : Context
        {
            Assert.IsNotNull(component);
            return (GetContext(component.gameObject, typeof(T), create) as T);
        }

        #endregion
    }
}

#endif
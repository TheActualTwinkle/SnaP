#if !NOT_UNITY3D

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModestTree;
using UnityEngine;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable ArrangeThisQualifier

namespace Zenject
{
    /// <summary>
    /// The script that automatically binds/injects dependencies in the dynamically instantiated game object 
    /// even if the game object was not instantiated throw Zenject resolving pipeline.
    /// </summary>
    /// <remarks>
    /// Automatically binding/injecting will be performed for every game object that this script is attached to. 
    /// Before injecting dependencies all bindings configured in ZenjectBinding scripts will be added to the Zenject 
    /// container like it happens during scene loading flow.On the game object destroys all bindings that were added 
    /// through ZenjectBinding scripts will be automatically unbinded.
    /// </remarks>
    public class ZenjectDynamicObjectInjection : MonoBehaviour
    {
        // We do not want to patch Zenject framework sources so we will use reflection to get to the required functionality.
        static readonly MethodInfo _contextInstallZenjectBindingMethod = typeof(Context).GetMethod("InstallZenjectBinding", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly Stack<List<ZenjectBinding>> _reusableZenjectBindingLists = new Stack<List<ZenjectBinding>>(new[] { new List<ZenjectBinding>() });

        Context _context;
        bool _isInjected;

        [Inject]
        // ReSharper disable once UnusedMember.Local
        void Initialize()
        {
            _isInjected = true;
        }

        void InstallZenjectBinding(ZenjectBinding binding)
        {
            _contextInstallZenjectBindingMethod.Invoke(_context, new object[] { binding });
        }

        void UninstallZenjectBinding(ZenjectBinding binding)
        {
            if (!binding.enabled)
            {
                return;
            }

            if (binding.Components == null)
            {
                return;
            }

            string identifier = null;

            if (binding.Identifier.Trim().Length > 0)
            {
                identifier = binding.Identifier;
            }

            DiContainer container = _context.Container;

            foreach (var component in binding.Components)
            {
                var bindType = binding.BindType;

                if (component == null)
                {
                    continue;
                }

                var componentType = component.GetType();

                switch (bindType)
                {
                    case ZenjectBinding.BindTypes.Self:
                    {
                        if (identifier != null)
                        {
                            container.UnbindId(componentType, identifier);
                        }
                        else
                        {
                            container.Unbind(componentType);
                        }
                        break;
                    }
                    case ZenjectBinding.BindTypes.BaseType:
                    {
                        if (identifier != null)
                        {
                            container.UnbindId(componentType.BaseType(), identifier);
                        }
                        else
                        {
                            container.Unbind(componentType.BaseType());
                        }
                        break;
                    }
                    case ZenjectBinding.BindTypes.AllInterfaces:
                    {
                        if (identifier != null)
                        {
                            foreach (var interfaceType in componentType.Interfaces())
                            {
                                container.UnbindId(interfaceType, identifier);
                            }
                        }
                        else
                        {
                            foreach (var interfaceType in componentType.Interfaces())
                            {
                                container.Unbind(interfaceType);
                            }
                        }
                        break;
                    }
                    case ZenjectBinding.BindTypes.AllInterfacesAndSelf:
                    {
                        if (identifier != null)
                        {
                            foreach (var interfaceType in componentType.Interfaces().Append(componentType))
                            {
                                container.UnbindId(interfaceType, identifier);
                            }
                        }
                        else
                        {
                            foreach (var interfaceType in componentType.Interfaces().Append(componentType))
                            {
                                container.Unbind(interfaceType);
                            }
                        }
                        break;
                    }
                }
            }
        }

        void Awake()
        {
            // Injection already happened no needs to inject two times.
            if (_isInjected)
            {
                return;
            }
            _isInjected = true;

            _context = ZenjectLocator.GetContext(this);
            Assert.IsNotNull(_context,
                "Could not find Context component for game objects '{0}'", name);
            if (_context == null)
            {
                return;
            }

            // First we need to install all binding.
            Assert.IsNotNull(_contextInstallZenjectBindingMethod,
                "Could not bind InstallZenjectBinding method of Context", name);
            if (_contextInstallZenjectBindingMethod != null)
            {
                var zenjectBindingList = (_reusableZenjectBindingLists.Count != 0) ? _reusableZenjectBindingLists.Pop() : new List<ZenjectBinding>();

                this.GetComponentsInChildren(true, zenjectBindingList);
                foreach (var binding in zenjectBindingList)
                {
                    InstallZenjectBinding(binding);
                }

                zenjectBindingList.Clear();
                _reusableZenjectBindingLists.Push(zenjectBindingList);
            }

            // Now we can inject dependencies.
            _context.Container.InjectGameObject(gameObject);
        }

        void OnDestroy()
        {
            if (!_isInjected)
            {
                return;
            }
            _isInjected = false;

            if (_context == null)
            {
                return;
            }

            if (_contextInstallZenjectBindingMethod != null)
            {
                var zenjectBindingList = (_reusableZenjectBindingLists.Count != 0) ? _reusableZenjectBindingLists.Pop() : new List<ZenjectBinding>();

                this.GetComponentsInChildren(true, zenjectBindingList);
                foreach (var binding in zenjectBindingList)
                {
                    UninstallZenjectBinding(binding);
                }

                zenjectBindingList.Clear();
                _reusableZenjectBindingLists.Push(zenjectBindingList);
            }
        }
    }
}

#endif
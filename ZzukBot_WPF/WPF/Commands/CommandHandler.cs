using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ZzukBot.WPF.Commands.Interfaces;

namespace ZzukBot.WPF.Commands
{
    internal class CommandHandler : IDisposable
    {
        private static readonly Dictionary<object, CommandHandler> Instances
            = new Dictionary<object, CommandHandler>();

        private readonly IDictionary<ICommand, ICommandAction> _actions;
        private readonly IDictionary<ICommand, CommandBinding> _bindings;

        internal CommandHandler(object viewModel)
        {
            Instances.Add(viewModel, this);
            _bindings = new Dictionary<ICommand, CommandBinding>();
            _actions = new Dictionary<ICommand, ICommandAction>();
            ManagedUiElements = new ObservableCollection<UIElement>();
            ManagedUiElements.CollectionChanged += OnUiElementCollectionChanged;
        }

        private ObservableCollection<UIElement> ManagedUiElements { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        internal void Register(string commandName, Action action)
        {
            var command = commandName.GetCommand();
            _actions.Add(command, new CommandWithoutParameter(action));
            AddToBindings(command);
        }

        internal void Register(string commandName, Func<bool> canExecute, Action action)
        {
            var command = commandName.GetCommand();
            _actions.Add(command, new CommandWithoutParameter(canExecute, action));
            AddToBindings(command);
        }

        internal void Register<T>(string commandName, Predicate<T> canExecute, Action<T> action) where T : class
        {
            var command = commandName.GetCommand();
            _actions.Add(command, new CommandWithParameter<T>(canExecute, action));
            AddToBindings(command);
        }

        internal void Register<T>(string commandName, Action<T> action) where T : class
        {
            var command = commandName.GetCommand();
            _actions.Add(command, new CommandWithParameter<T>(x => true, action));
            AddToBindings(command);
        }

        private void AddToBindings(ICommand command)
        {
            var binding = new CommandBinding(command);
            binding.CanExecute += CommandCanExecuteEventHandler;
            binding.Executed += CommandExecuteEventHandler;
            _bindings.Add(command, binding);

            foreach (var element in ManagedUiElements)
                element.CommandBindings.Add(binding);
        }

        ~CommandHandler()
        {
            Dispose(false);
        }

        private void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;
            var viewModel = Instances
                .FirstOrDefault(x => x.Value == this)
                .Key;
            if (viewModel != null)
                Instances.Remove(viewModel);
            ManagedUiElements.CollectionChanged
                -= OnUiElementCollectionChanged;

            foreach (var x in _actions)
            {
                var binding = _bindings[x.Key];
                binding.CanExecute -= CommandCanExecuteEventHandler;
                binding.Executed -= CommandExecuteEventHandler;
                _bindings.Remove(x.Key);
                foreach (var element in ManagedUiElements)
                    element.CommandBindings.Remove(binding);
            }
        }

        private void OnUiElementCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (UIElement element in e.NewItems)
                foreach (var binding in _bindings.Values)
                    element.CommandBindings.Add(binding);
            // ReSharper disable once InvertIf
            if (e.OldItems != null)
                foreach (UIElement element in e.OldItems)
                foreach (var binding in _bindings.Values)
                    element.CommandBindings.Remove(binding);
        }

        private void CommandCanExecuteEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            var action = _actions[e.Command];
            e.CanExecute = action.CanExecute(e.Parameter);
            e.Handled = true;
        }

        private void CommandExecuteEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var action = _actions[e.Command];
            action.Execute(e.Parameter);
            e.Handled = true;
        }

        internal static void RegisterUiElement(UIElement element, object viewModel)
        {
            if (Instances.ContainsKey(viewModel))
                Instances[viewModel].ManagedUiElements.Add(element);
        }

        internal static void UnregisterUieLement(UIElement element, object viewModel)
        {
            if (Instances.ContainsKey(viewModel))
                Instances[viewModel].ManagedUiElements.Remove(element);
        }
    }
}
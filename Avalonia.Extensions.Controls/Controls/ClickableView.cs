﻿using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System;
using System.Linq;
using System.Windows.Input;

namespace Avalonia.Extensions.Controls
{
    /// <summary>
    /// This is a clickable <seealso cref="Panel" />,
    /// Just like the <seealso cref="Button"/> in 
    /// <a href="https://avaloniaui.net/docs/controls/button#play-button">this instance</a>.
    /// </summary>
    [PseudoClasses(":pressed")]
    public class ClickableView : Panel, ICommandSource
    {
        static ClickableView()
        {
            ClipToBoundsProperty.OverrideDefaultValue<ClickableView>(true);
            FocusableProperty.OverrideDefaultValue(typeof(ClickableView), true);
            CommandProperty.Changed.Subscribe(CommandChanged);
            IsDefaultProperty.Changed.Subscribe(IsDefaultChanged);
            IsCancelProperty.Changed.Subscribe(IsCancelChanged);
        }
        public ClickableView()
        {
            UpdatePseudoClasses(IsPressed);
        }
        private ICommand _command;
        private bool _commandCanExecute = true;
        public bool IsDefault
        {
            get => GetValue(IsDefaultProperty);
            set => SetValue(IsDefaultProperty, value);
        }
        public static readonly StyledProperty<bool> IsDefaultProperty =
            AvaloniaProperty.Register<ClickableView, bool>(nameof(IsDefault));
        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }
        public static readonly StyledProperty<Thickness> PaddingProperty =
           Decorator.PaddingProperty.AddOwner<ClickableView>();
        public ICommand Command
        {
            get => _command;
            set => SetAndRaise(CommandProperty, ref _command, value);
        }
        public static readonly DirectProperty<ClickableView, ICommand> CommandProperty =
          AvaloniaProperty.RegisterDirect<ClickableView, ICommand>(nameof(Command),
              content => content.Command, (content, command) => content.Command = command, enableDataValidation: true);
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly StyledProperty<object> CommandParameterProperty =
           AvaloniaProperty.Register<ClickableView, object>(nameof(CommandParameter));
        public event EventHandler<RoutedEventArgs> Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
           RoutedEvent.Register<ClickableView, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
        public ClickMode ClickMode
        {
            get => GetValue(ClickModeProperty);
            set => SetValue(ClickModeProperty, value);
        }
        public static readonly StyledProperty<ClickMode> ClickModeProperty =
            AvaloniaProperty.Register<ClickableView, ClickMode>(nameof(ClickMode), ClickMode.Press);
        public bool IsPressed
        {
            get => GetValue(IsPressedProperty);
            private set => SetValue(IsPressedProperty, value);
        }
        public static readonly StyledProperty<bool> IsPressedProperty =
           AvaloniaProperty.Register<ClickableView, bool>(nameof(IsPressed));
        public bool IsCancel
        {
            get => GetValue(IsCancelProperty);
            set => SetValue(IsCancelProperty, value);
        }
        public static readonly StyledProperty<bool> IsCancelProperty =
            AvaloniaProperty.Register<ClickableView, bool>(nameof(IsCancel));
        public void CanExecuteChanged(object sender, EventArgs e)
        {
            var canExecute = Command == null || Command.CanExecute(CommandParameter);
            if (canExecute != _commandCanExecute)
            {
                _commandCanExecute = canExecute;
                UpdateIsEffectivelyEnabled();
            }
        }
        protected virtual void OnClick()
        {
            var e = new RoutedEventArgs(ClickEvent);
            RaiseEvent(e);
            if (!e.Handled && Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
                e.Handled = true;
            }
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (IsDefault)
            {
                if (e.Root is IInputElement inputElement)
                    ListenForDefault(inputElement);
            }
            if (IsCancel)
            {
                if (e.Root is IInputElement inputElement)
                    ListenForCancel(inputElement);
            }
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            if (IsDefault)
            {
                if (e.Root is IInputElement inputElement)
                    StopListeningForDefault(inputElement);
            }
        }
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            if (Command != null)
            {
                Command.CanExecuteChanged += CanExecuteChanged;
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            if (Command != null)
                Command.CanExecuteChanged -= CanExecuteChanged;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnClick();
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                if (ClickMode == ClickMode.Press)
                    OnClick();
                IsPressed = true;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (ClickMode == ClickMode.Release)
                    OnClick();
                IsPressed = false;
                e.Handled = true;
            }
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                IsPressed = true;
                e.Handled = true;
                if (ClickMode == ClickMode.Press)
                    OnClick();
            }
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (IsPressed && e.InitialPressMouseButton == MouseButton.Left)
            {
                IsPressed = false;
                e.Handled = true;
                if (ClickMode == ClickMode.Release && this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)))
                    OnClick();
            }
        }
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            IsPressed = false;
        }
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsPressedProperty)
                UpdatePseudoClasses(change.NewValue.GetValueOrDefault<bool>());
        }
        private void UpdatePseudoClasses(bool isPressed)
        {
            PseudoClasses.Set(":pressed", isPressed);
        }
        protected override void UpdateDataValidation<T>(AvaloniaProperty<T> property, BindingValue<T> value)
        {
            base.UpdateDataValidation(property, value);
            if (property == CommandProperty)
            {
                if (value.Type == BindingValueType.BindingError)
                {
                    if (_commandCanExecute)
                    {
                        _commandCanExecute = false;
                        UpdateIsEffectivelyEnabled();
                    }
                }
            }
        }
        private static void CommandChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is ClickableView content)
            {
                if (((ILogical)content).IsAttachedToLogicalTree)
                {
                    if (e.OldValue is ICommand oldCommand)
                        oldCommand.CanExecuteChanged -= content.CanExecuteChanged;
                    if (e.NewValue is ICommand newCommand)
                        newCommand.CanExecuteChanged += content.CanExecuteChanged;
                }
                content.CanExecuteChanged(content, EventArgs.Empty);
            }
        }
        private static void IsDefaultChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var content = e.Sender as ClickableView;
            var isDefault = (bool)e.NewValue;
            if (content?.VisualRoot is IInputElement inputRoot)
            {
                if (isDefault)
                    content.ListenForDefault(inputRoot);
                else
                    content.StopListeningForDefault(inputRoot);
            }
        }
        private static void IsCancelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var content = e.Sender as ClickableView;
            var isCancel = (bool)e.NewValue;
            if (content?.VisualRoot is IInputElement inputRoot)
            {
                if (isCancel)
                    content.ListenForCancel(inputRoot);
                else
                    content.StopListeningForCancel(inputRoot);
            }
        }
        private void ListenForDefault(IInputElement root)
        {
            root.AddHandler(KeyDownEvent, RootDefaultKeyDown);
        }
        private void ListenForCancel(IInputElement root)
        {
            root.AddHandler(KeyDownEvent, RootCancelKeyDown);
        }
        private void StopListeningForDefault(IInputElement root)
        {
            root.RemoveHandler(KeyDownEvent, RootDefaultKeyDown);
        }
        private void StopListeningForCancel(IInputElement root)
        {
            root.RemoveHandler(KeyDownEvent, RootCancelKeyDown);
        }
        private void RootCancelKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && IsVisible && IsEnabled)
                OnClick();
        }
        private void RootDefaultKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && IsVisible && IsEnabled)
                OnClick();
        }
        void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged(sender, e);
    }
}
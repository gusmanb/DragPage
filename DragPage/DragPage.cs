/********************************************************************************/
//Copyright 2013 Agustin Gimenez Bernad
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
/********************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinRTXamlToolkit.Composition;

namespace DragControls
{

    /// <summary>
    /// DragPage is a page which allows to do Drag and Drop in ANY control
    /// </summary>
    public abstract class DragPage : Page
    {

        /// <summary>
        /// Raised when a drag starts.
        /// Set the Cancel property in the arguments to cancel the drag
        /// </summary>
        public event EventHandler<DragEventArgs> DragBegin;

        /// <summary>
        /// Raised when a drop over a target is done
        /// </summary>
        public event EventHandler<DragEventArgs> DragEnd;

        private bool dragging = false;
        private FrameworkElement source = null;

        /// <summary>
        /// Controls if the drag functions are enabled or disabled in this page
        /// </summary>
        public bool EnableDragging
        {
            get { return (bool)GetValue(EnableDraggingProperty); }
            set { SetValue(EnableDraggingProperty, value); }
        }

        /// <summary>
        /// Register of the dependency property "EnableDragging"
        /// </summary>
        public static readonly DependencyProperty EnableDraggingProperty =
            DependencyProperty.Register("EnableDragging", typeof(bool), typeof(DragPage), new PropertyMetadata(false));

        /// <summary>
        /// Register of the attached property "IsDragable"
        /// This property enables or disables a control as draggable
        /// </summary>
        public static readonly DependencyProperty IsDragableProperty =
            DependencyProperty.Register("IsDragable", typeof(bool), typeof(DragPage), new PropertyMetadata(false, DragableSet));

        /// <summary>
        /// IsDragable getter
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>The stored value</returns>
        public static bool GetIsDragable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragableProperty);
        }

        /// <summary>
        /// IsDragable setter
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="value">The value to store</param>
        public static void SetIsDragable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragableProperty, value);
        }

        /// <summary>
        /// Register of the attached property "IsDropTarget"
        /// This property enables or disables a control as a drop target
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.Register("IsDropTarget", typeof(bool), typeof(DragPage), new PropertyMetadata(false, TargetSet));

        /// <summary>
        /// IsDropTarget getter
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>The stored value</returns>
        public static bool GetIsDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDropTargetProperty);
        }

        /// <summary>
        /// IsDropTarget setter
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="value">The value to store</param>
        public static void SetIsDropTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDropTargetProperty, value);
        }

        private List<FrameworkElement> dragableObjects = new List<FrameworkElement>();
        private List<FrameworkElement> dragableTargets = new List<FrameworkElement>();
        
        static void DragableSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (e.OldValue == e.NewValue)
                return;

            FrameworkElement fw = d as FrameworkElement;

            DragPage page = FindPage(fw);


            //If the property is set before the control is loaded in 
            //it's parent, the parent page can not be found so we 
            //hook to the loaded event and delay the assignment
            if (page == null)
            {

                if ((bool)e.NewValue)
                    fw.Loaded += DragableLoaded;
                else
                    fw.Loaded -= DragableLoaded;

            }
            else
            {

                if ((bool)fw.GetValue(DragPage.IsDragableProperty) == true)
                {
                    page.dragableObjects.Add(fw);
                    fw.PointerPressed += BeginDrag;

                }
                else
                {
                    page.dragableObjects.Remove(fw);
                    fw.PointerPressed -= BeginDrag;
                }

                fw.Loaded -= DragableLoaded;

            }
        }

        static void DragableLoaded(object sender, RoutedEventArgs e)
        {

            //The property was set before the control
            //was loaded, now we can seek for it's parent

            FrameworkElement fw = sender as FrameworkElement;
            DragPage page = FindPage(fw);

            if (page == null)
                return;

            if ((bool)fw.GetValue(DragPage.IsDragableProperty) == true)
            {
                page.dragableObjects.Add(fw);
                fw.PointerPressed += BeginDrag;

            }
            else
            {
                page.dragableObjects.Remove(fw);
                fw.PointerPressed -= BeginDrag;
            }

            fw.Loaded -= DragableLoaded;
        }

        static void TargetSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (e.OldValue == e.NewValue)
                return;

            FrameworkElement fw = d as FrameworkElement;

            DragPage page = FindPage(fw);

            //If the property is set before the control is loaded in 
            //it's parent, the parent page can not be found so we 
            //hook to the loaded event and delay the assignment
            if (page == null)
            {
                if ((bool)e.NewValue)
                    fw.Loaded += DropTargetLoaded;
                else
                    fw.Loaded -= DropTargetLoaded;
            }
            else
            {
                if ((bool)fw.GetValue(DragPage.IsDropTargetProperty) == true)
                    page.dragableTargets.Add(fw);
                else
                    page.dragableTargets.Remove(fw);

                fw.Loaded -= DropTargetLoaded;
            }
        }

        static void DropTargetLoaded(object sender, RoutedEventArgs e)
        {
            //The property was set before the control
            //was loaded, now we can seek for it's parent

            FrameworkElement fw = sender as FrameworkElement;
            DragPage page = FindPage(fw);

            if (page == null)
                return;

            if ((bool)fw.GetValue(DragPage.IsDropTargetProperty) == true)
                page.dragableTargets.Add(fw);
            else
                page.dragableTargets.Remove(fw);

            fw.Loaded -= DropTargetLoaded;
        
        }

        /// <summary>
        /// Called when the pointer is released
        /// </summary>
        static void EndDrag(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
                return;

            DragPage page = FindPage(img);

            if (page == null || !page.EnableDragging || !page.dragging)
                return;

            Point pt = e.GetCurrentPoint(page).Position;

            Panel container = page.Content as Panel;

            container.Children.Remove(img);

            img.PointerMoved -= DeltaDrag;
            img.PointerReleased -= EndDrag;

            if(page.DragEnd != null)
            {

                foreach (FrameworkElement elem in page.dragableTargets)
                {

                    Rect bounds = GetBounds(elem, page);

                    if (bounds.Contains(pt))
                    {
                        page.DragEnd(page, new DragEventArgs { Source = page.source, Target = elem, Cancel = false });
                        break;
                    }
                }
            }

            page.dragging = false;
        }

        /// <summary>
        /// Called when the pointer has moved and is captured
        /// </summary>
        static void DeltaDrag(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

            Image img = sender as Image;

            if (img == null)
                return;

            DragPage page = FindPage(img);

            if (page == null || !page.EnableDragging || !page.dragging)
                return;

            Point pt = e.GetCurrentPoint(page).Position;

            TranslateTransform imgTransform = img.RenderTransform as TranslateTransform;

            imgTransform.X = pt.X - img.Width / 2;
            imgTransform.Y = pt.Y - img.Height / 2;

        }

        /// <summary>
        /// Called when the pointer is pressed
        /// </summary>
        static void BeginDrag(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

            FrameworkElement fw = sender as FrameworkElement;

            if (fw == null)
                return;

            DragPage page = FindPage(fw);

            if (page == null)
                return;

            page.BeginItemDrag(fw, e.Pointer);
        }

        /// <summary>
        /// Used to begin manually a drag operatio.
        /// Usually called from the event DragBegin after cancelling it
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="Pointer"></param>
        public void BeginItemDrag(FrameworkElement Element, Pointer Pointer)
        {

            FrameworkElement fw = Element;
            DragPage page = this;

            if (page == null || !page.EnableDragging || page.dragging)
                return;

            DragEventArgs data = new DragEventArgs { Source = fw, Cancel = false };

            if (page.DragBegin != null)
                page.DragBegin(page, data);

            if (data.Cancel)
                return;

            page.dragging = true;

            MemoryStream ms = WinRTXamlToolkit.Composition.WriteableBitmapRenderExtensions.RenderToStream(fw);
            MemoryRandomAccessStream mms = new MemoryRandomAccessStream(ms);
            WriteableBitmap bmp = new WriteableBitmap(1, 1);
            bmp.SetSource(mms);
            bmp.Invalidate();
            mms.Dispose();

            Image img = new Image();
            img.Source = bmp;

            Rect location = GetBounds(fw, page);

            img.Width = location.Width;
            img.Height = location.Height;

            img.Opacity = 0.5;

            Panel container = page.Content as Panel;

            if (page.Content is Canvas)
                img.SetValue(Canvas.ZIndexProperty, 10000);

            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.VerticalAlignment = VerticalAlignment.Top;
            img.Margin = new Thickness(0);

            TranslateTransform imgTransform = new TranslateTransform();
            img.RenderTransform = imgTransform;

            imgTransform.X = location.X;
            imgTransform.Y = location.Y;

            container.Children.Add(img);

            img.CapturePointer(Pointer);
            img.PointerMoved += DeltaDrag;
            img.PointerReleased += EndDrag;

            page.source = fw;
        
        }

        /// <summary>
        /// Gets the relative bounds of a control
        /// </summary>
        /// <param name="Control">Control to find bounds</param>
        /// <param name="Container">Target control</param>
        /// <returns></returns>
        static Rect GetBounds(FrameworkElement Control, FrameworkElement Container)
        {
            var transform = Control.TransformToVisual(Container);
            Rect bounds = transform.TransformBounds(new Rect(0, 0, Control.ActualWidth, Control.ActualHeight));
            return bounds;
        
        }

        /// <summary>
        /// Finds a FrameworkElement main page
        /// </summary>
        /// <param name="fw">The element to find it's parent</param>
        /// <returns>If it's a DragPage returns the page, else returns null</returns>
        static DragPage FindPage(FrameworkElement fw)
        {
        
            if (fw == null)
                return null;

            DragPage page = null;
            FrameworkElement parent = fw.Parent as FrameworkElement;
            FrameworkElement container = null;

            while (page == null && parent != null)
            {

                if (parent is DragPage)
                    page = (DragPage)parent;
                else
                    parent = parent.Parent as FrameworkElement;
           
            }

            if (page != null && page.Content is Panel)
                container = page.Content as FrameworkElement;

            if (container == null)
                return null;

            return page;
        
        }
              
        
    }

    /// <summary>
    /// Drag events arguments
    /// </summary>
    public sealed class DragEventArgs : EventArgs
    {

        /// <summary>
        /// Source of the operation, always filled
        /// </summary>
        public FrameworkElement Source { get; set; }
        /// <summary>
        /// Target of the operation, only filled in DragEnd
        /// </summary>
        public FrameworkElement Target { get; set; }
        /// <summary>
        /// Pointer captured for the operation, only filled in DragBegin
        /// </summary>
        public Pointer Pointer { get; set; }
        /// <summary>
        /// Cancellation of the event, only used in DragStart
        /// </summary>
        public bool Cancel { get; set; }
    
    }
}

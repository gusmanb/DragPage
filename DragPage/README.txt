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

DragPage 1.0
-------------

*What is this?

DragPage is a WinRT component which inherits from Page and allows to use drag and drop in ANY control contained inside it.

*Requirements

Visual Studio 2012
The WinRT XAML Toolkit (https://winrtxamltoolkit.codeplex.com/)

*How it works?

The basics are really simple:

	1-Add the DragPage.IsDragable property to the controls you want to drag.
	2-Add the DragPage.IsDropTarget to the controls who admit drop.
	3-Attach to the DragBegin event if necessary. This event allows to cancel a drag before it starts.
	4-Attach to the DragEnd event. The arguments hold the source and the target of the operation.
	5-Done!

*Known limitations?

-Drag is limied to one object simultaneously (you cannot drag multiple items) BUT, you can drag ANY control, so you can group your controls inside a Panel and drag it (see BeginItemDrag in DragPage).
-If the dragged item has a RenderTransform applied to it, the dragged image will be incorrect.
-The page root container must be a panel (grid, canvas...) or it will not work.

*Why you did this?

Ok, i'm programming some Windows Store applications, and in one of them I wanted to drag and drop some images.
But Oh, Oh! in Windows Store apps, only the GridView and ListView can do this!! WTF!!!

That's really nice, so, if you want a layout which is not a ListView or GridView you can just go to sh*t without passing through the middle, or code it by yourself.

My first try was to wrap each item inside a GridView. It wasn't elegant, but it seemed to work. Notate the *seemed* word.

After coding everything and sent it to the store for certification, it was rejected because the drag and drop did not worked using touches.

Ok, nice, a property to disable drag and drop only for touches? Well Done! XD

So I enabled the "IsSwipeEnabled" (think it was) and it started to drag, but hell!! what a crap of drag!!
It only works for the perpendicular direction to the inner ScrollViewer scroll direction of the GridView/ListView.

One can think "ok, so, if I disable the ScrollViewer scrolls, it should drag in both axes, no?" WRONG!!
If the ScrollViewer horizontal scroll and vertical scroll are disabled, the drag and drop stops working, and the same happens if both are active.

So, finally I gave up and did the DragPage.

It worked so fine that I had to publish it and let anyone who needs to use it, so here it is.

Hope it helps you!


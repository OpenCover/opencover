    /**
    * o--------------------------------------------------------------------------------o
    * | This file is part of the RGraph package. RGraph is Free Software, licensed     |
    * | under the MIT license - so it's free to use for all purposes. If you want to   |
    * | donate to help keep the project going then you can do so here:                 |
    * |                                                                                |
    * |                             http://www.rgraph.net/donate                       |
    * o--------------------------------------------------------------------------------o
    */

    /**
    * Initialise the various objects
    */
    RGraph = window.RGraph || {isRGraph: true};




// Module pattern
(function (win, doc, undefined)
{
    var RG = RGraph,
        ua = navigator.userAgent,
        ma = Math;




    /**
    * This is the window click event listener. It redraws all canvas tags on the page.
    */
    RGraph.installWindowMousedownListener =
    RGraph.InstallWindowMousedownListener = function (obj)
    {
        if (!RGraph.window_mousedown_event_listener) {

            RGraph.window_mousedown_event_listener = function (e)
            {
                /**
                * For firefox add the window.event object
                */
                if (navigator.userAgent.indexOf('Firefox') >= 0) win.event = e;
                
                e = RGraph.FixEventObject(e);
    

                if (RGraph.HideTooltip && RGraph.Registry.Get('chart.tooltip')) {
                    RGraph.clear(RGraph.Registry.Get('chart.tooltip').__canvas__);
                    RGraph.redraw();
                    RGraph.hideTooltip();
                }
            };
            win.addEventListener('mousedown', RGraph.window_mousedown_event_listener, false);
        }
    };




    /**
    * This is the window click event listener. It redraws all canvas tags on the page.
    */
    RGraph.installWindowMouseupListener =
    RGraph.InstallWindowMouseupListener = function (obj)
    {
        if (!RGraph.window_mouseup_event_listener) {
            RGraph.window_mouseup_event_listener = function (e)
            {
                /**
                * For firefox add the window.event object
                */
                if (navigator.userAgent.indexOf('Firefox') >= 0) win.event = e;
                
                e = RGraph.FixEventObject(e);
    
    
                /**
                * Stop any annotating that may be going on
                */
                if (RGraph.Annotating_window_onmouseup) {
                    RGraph.Annotating_window_onmouseup(e);
                    return;
                }
    
                /**
                * End adjusting
                */
                if (RGraph.Registry.Get('chart.adjusting') || RGraph.Registry.Get('chart.adjusting.gantt')) {
                    RGraph.FireCustomEvent(RGraph.Registry.Get('chart.adjusting'), 'onadjustend');
                }
    
                RGraph.Registry.Set('chart.adjusting', null);
                RGraph.Registry.Set('chart.adjusting.shape', null);
                RGraph.Registry.Set('chart.adjusting.gantt', null);
    
    
                // ==============================================
                // Finally, redraw the chart
                // ==============================================

                var tags = document.getElementsByTagName('canvas');
                for (var i=0; i<tags.length; ++i) {
                    if (tags[i].__object__ && tags[i].__object__.isRGraph) {
                        if (!tags[i].__object__.Get('chart.annotatable')) {
                            if (!tags[i].__rgraph_trace_cover__ && !noredraw) {
                                RGraph.Clear(tags[i]);
                            } else {
                                var noredraw = true;
                            }
                        }
                    }
                }
    
                if (!noredraw) {
                    RGraph.Redraw();
                }
            };
            win.addEventListener('mouseup', RGraph.window_mouseup_event_listener, false);
        }
    };




    /**
    * This is the canvas mouseup event listener. It installs the mouseup event for the
    * canvas. The mouseup event then checks the relevant object.
    * 
    * @param object obj The chart object
    * 
    * RGraph.window_mouseup_event_listener
    */
    RGraph.installCanvasMouseupListener =
    RGraph.InstallCanvasMouseupListener = function (obj)
    {
        if (!obj.canvas.rgraph_mouseup_event_listener) {
            obj.canvas.rgraph_mouseup_event_listener = function (e)
            {
                /**
                * For firefox add the window.event object
                */
                if (navigator.userAgent.indexOf('Firefox') >= 0) window.event = e;
    
                e = RGraph.FixEventObject(e);
    
    
                // *************************************************************************
                // Tooltips
                // *************************************************************************
    
    
                // This causes things at the edge of the chart area - eg line chart hotspots - not to fire because the
                // cursor is out of the chart area
                var objects = RGraph.ObjectRegistry.getObjectsByXY(e);
                //var objects = RGraph.ObjectRegistry.getObjectsByCanvasID(e.target.id);

                if (objects) {
                    for (var i=0,len=objects.length; i<len; i+=1) {
                        
                        var obj = objects[i];
                        var id  = objects[i].id;

    
                        // =========================================================================
                        // The drawing API text object supports chart.link
                        // ========================================================================
                        var link = obj.Get('link');
                        
                        if (obj.type == 'drawing.text' && typeof link === 'string') {

                            var link_target  = obj.Get('link.target');
                            var link_options = obj.Get('link.options');

                            window.open(link, link_target ? link_target : null, link_options);
                        }

    
                        // ========================================================================
                        // Tooltips
                        // ========================================================================
    
                    
                        if (!RGraph.is_null(obj) && RGraph.Tooltip) {
    
                            var shape = obj.getShape(e);
    
                            if (shape && shape['tooltip']) {
    
                                var text = shape['tooltip'];
    
                                if (text) {
    
                                    var type = shape['object'].type;
    
                                    if (   type == 'line'
                                        || type == 'rscatter'
                                        || (type == 'scatter' && !obj.Get('chart.boxplot'))
                                        || type == 'radar') {
    
                                        var canvasXY = RGraph.getCanvasXY(obj.canvas);
                                        var x = canvasXY[0] + shape['x'];
                                        var y = canvasXY[1] + shape['y'];
    
                                    } else {
                                        var x = e.pageX;
                                        var y = e.pageY;
                                    }

                                    RGraph.Clear(obj.canvas);
                                    RGraph.Redraw();
                                    obj.Highlight(shape);
                                    RGraph.Registry.Set('chart.tooltip.shape', shape);
                                    RGraph.Tooltip(obj, text, x, y, shape['index'], e);
    
                                    // Add the shape that triggered the tooltip
                                    if (RGraph.Registry.Get('chart.tooltip')) {
                                        
                                        RGraph.Registry.Get('chart.tooltip').__shape__ = shape;
    
                                        RGraph.EvaluateCursor(e);
                                    }
    
                                    e.cancelBubble = true;
                                    e.stopPropagation();
                                    return false;
                                }
                            }
                        }
    
    
    
    
    
                        // =========================================================================
                        // Adjusting
                        // ========================================================================
        
        
        
                        if (RGraph.Registry.Get('chart.adjusting') || RGraph.Registry.Get('chart.adjusting.gantt')) {
                            RGraph.FireCustomEvent(RGraph.Registry.Get('chart.adjusting'), 'onadjustend');
                        }
        
                        RGraph.Registry.Set('chart.adjusting', null);
                        RGraph.Registry.Set('chart.adjusting.shape', null);
                        RGraph.Registry.Set('chart.adjusting.gantt', null);
    
                        /**
                        * If the mouse pointer is over a "front" chart this prevents charts behind it
                        * from firing their events.
                        */
                        if (shape || (obj.overChartArea && obj.overChartArea(e)) ) {
                            break;
                        }
                    }
                }
            };
            obj.canvas.addEventListener('mouseup', obj.canvas.rgraph_mouseup_event_listener, false);
        }
    };




    /**
    * This is the canvas mousemove event listener.
    * 
    * @param object obj The chart object
    */
    RGraph.installCanvasMousemoveListener =
    RGraph.InstallCanvasMousemoveListener = function (obj)
    {
        if (!obj.canvas.rgraph_mousemove_event_listener) {
            obj.canvas.rgraph_mousemove_event_listener = function (e)
            {
                /**
                * For firefox add the window.event object
                */
                if (navigator.userAgent.indexOf('Firefox') >= 0) window.event = e;
                e = RGraph.FixEventObject(e);

    
    
    
                /**
                * Go through all the objects and check them to see if anything needs doing
                */
                var objects = RGraph.ObjectRegistry.getObjectsByXY(e);
                //var objects = RGraph.ObjectRegistry.getObjectsByCanvasID(e.target.id);

                if (objects && objects.length > 0) {
                    for (var i=0,len=objects.length; i<len; i+=1) {
    
                        var obj = objects[i];
                        var id  = obj.id;

                        if (!obj.getShape) {
                            continue;
                        }
    

                        var shape = obj.getShape(e);




                        // ================================================================================================ //
                        // This facilitates the chart.events.mousemove option
                        // ================================================================================================ //
                        
                        var func = obj.Get('chart.events.mousemove');
    
                        if (!func && typeof obj.onmousemove == 'function') {
                            var func = obj.onmousemove;
                        }
    
                        /**
                        * 
                        */
                        if (shape) {
                            var index = shape['object'].type == 'scatter' ? shape['index_adjusted'] : shape['index'];
                            if (typeof(obj['$' + index]) == 'object' && typeof(obj['$' + index].onmousemove) == 'function') {
                                var func2 = obj['$' + index].onmousemove;
                            }
                        }
    
                        /**
                        * This bit saves the current pointer style if there isn't one already saved
                        */
                        if (shape && (typeof(func) == 'function' || typeof(func2) == 'function' || typeof obj.Get('link') === 'string')) {
    
                            if (obj.Get('chart.events.mousemove.revertto') == null) {
                                obj.Set('chart.events.mousemove.revertto', e.target.style.cursor);
                            }
    
                            if (typeof(func)  == 'function')  func(e, shape);
                            if (typeof(func2) == 'function') func2(e, shape);

    
                            //return;
    
                        } else if (typeof(obj.Get('chart.events.mousemove.revertto')) == 'string') {
            
                            RGraph.cursor.push('default');
                            obj.Set('chart.events.mousemove.revertto', null);
                        }
    
    
    
                        // ================================================================================================ //
                        // Tooltips
                        // ================================================================================================ //
    

                        if (   shape
                            && (obj.Get('chart.tooltips') && obj.Get('chart.tooltips')[shape['index']] || shape['tooltip'])
                            && (obj.Get('chart.tooltips.event') == 'onmousemove' || obj.Get('chart.tooltips.event') == 'mousemove')
                            && (RGraph.is_null(RGraph.Registry.Get('chart.tooltip')) || RGraph.Registry.Get('chart.tooltip').__index__ != shape['index'] || (typeof(shape['dataset']) == 'number' && shape['dataset'] != RGraph.Registry.Get('chart.tooltip').__shape__['dataset']) || obj.uid != RGraph.Registry.Get('chart.tooltip').__object__.uid)
                           ) {

                            RGraph.Clear(obj.canvas);
                            RGraph.Redraw();
                            obj.canvas.rgraph_mouseup_event_listener(e);
    
                            return;
                        }
            
            
                        // ================================================================================================ //
                        // Adjusting
                        // ================================================================================================ //
            

                        if (obj && obj.Get('chart.adjustable')) {
                            obj.Adjusting_mousemove(e);
                        }
                    
    
                        /**
                        * This facilitates breaking out of the loop when a shape has been found - 
                        * ie the cursor is over a shape an upper chart
                        */
                        if (shape || (obj.overChartArea && obj.overChartArea(e) )) {
                            break;
                        }
                    }
                }
    
                // ================================================================================================ //
                // Crosshairs
                // ================================================================================================ //
    

                if (e.target && e.target.__object__ && e.target.__object__.Get('chart.crosshairs')) {
                    RGraph.DrawCrosshairs(e, e.target.__object__);
                }
            
            
                // ================================================================================================ //
                // Interactive key No LONGER REQUIRED
                // ================================================================================================ //
    
    
                //if (typeof InteractiveKey_line_mousemove == 'function') InteractiveKey_line_mousemove(e);
                //if (typeof InteractiveKey_pie_mousemove == 'function') InteractiveKey_pie_mousemove(e);
    
    
                // ================================================================================================ //
                // Annotating
                // ================================================================================================ //
    
    
                if (e.target.__object__ && e.target.__object__.Get('chart.annotatable') && RGraph.Annotating_canvas_onmousemove) {
                    RGraph.Annotating_canvas_onmousemove(e);
                }
    
    
    
                /**
                * Determine the pointer
                */
                RGraph.EvaluateCursor(e);
            };
            obj.canvas.addEventListener('mousemove', obj.canvas.rgraph_mousemove_event_listener, false);
        }
    };




    /**
    * This is the canvas mousedown event listener.
    * 
    * @param object obj The chart object
    */
    RGraph.installCanvasMousedownListener =
    RGraph.InstallCanvasMousedownListener = function (obj)
    {
        if (!obj.canvas.rgraph_mousedown_event_listener) {
            obj.canvas.rgraph_mousedown_event_listener = function (e)
            {
                /**
                * For firefox add the window.event object
                */
                if (navigator.userAgent.indexOf('Firefox') >= 0) window.event = e;
                
                e = RGraph.FixEventObject(e);

    
                /**
                * Annotating
                */
                if (e.target.__object__ && e.target.__object__.Get('chart.annotatable') && RGraph.Annotating_canvas_onmousedown) {
                    RGraph.Annotating_canvas_onmousedown(e);
                    return;
                }
    
                var obj = RGraph.ObjectRegistry.getObjectByXY(e);
    
                if (obj) {

                    var id = obj.id;

                    /*************************************************************
                    * Handle adjusting for all object types
                    *************************************************************/
                    if (obj && obj.isRGraph && obj.Get('chart.adjustable')) {
                        
                        /**
                        * Check the cursor is in the correct area
                        */
                        var obj = RGraph.ObjectRegistry.getObjectByXY(e);
    
                        if (obj && obj.isRGraph) {
                        
                            // If applicable, get the appropriate shape and store it in the registry
                            switch (obj.type) {
                                case 'bar':   var shape = obj.getShapeByX(e); break;
                                case 'gantt':
                                    var shape = obj.getShape(e);
                                    if (shape) {
                                        var mouseXY = RGraph.getMouseXY(e);
                                        RGraph.Registry.Set('chart.adjusting.gantt', {
                                                                                      'index': shape['index'],
                                                                                      'object': obj,
                                                                                      'mousex': mouseXY[0],
                                                                                      'mousey': mouseXY[1],
                                                                                      'event_start': obj.data[shape['index']][0],
                                                                                      'event_duration': obj.data[shape['index']][1],
                                                                                      'mode': (mouseXY[0] > (shape['x'] + shape['width'] - 5) ? 'resize' : 'move'),
                                                                                      'shape': shape
                                                                                     });
                                    }
                                    break;
                                case 'line':  var shape = obj.getShape(e); break;
                                default:      var shape = null;
                            }
    
                            RGraph.Registry.Set('chart.adjusting.shape', shape);
    
    
                            // Fire the onadjustbegin event
                            RGraph.FireCustomEvent(obj, 'onadjustbegin');
    
                            RGraph.Registry.Set('chart.adjusting', obj);
        
    
                            // Liberally redraw the canvas
                            RGraph.Clear(obj.canvas);
                            RGraph.Redraw();
        
                            // Call the mousemove event listener so that the canvas is adjusted even though the mouse isn't moved
                            obj.canvas.rgraph_mousemove_event_listener(e);
                        }
                    }
    
    
                    RGraph.Clear(obj.canvas);
                    RGraph.Redraw();
                }
            };
            obj.canvas.addEventListener('mousedown', obj.canvas.rgraph_mousedown_event_listener, false);
        }
    };




    /**
    * This is the canvas click event listener. Used by the pseudo event listener
    * 
    * @param object obj The chart object
    */
    RGraph.installCanvasClickListener =
    RGraph.InstallCanvasClickListener = function (obj)
    {
        if (!obj.canvas.rgraph_click_event_listener) {
            obj.canvas.rgraph_click_event_listener = function (e)
            {
                /**
                * For firefox add the window.event object
                */
                if (navigator.userAgent.indexOf('Firefox') >= 0) window.event = e;
                
                e = RGraph.FixEventObject(e);
    
                var objects = RGraph.ObjectRegistry.getObjectsByXY(e);

                for (var i=0,len=objects.length; i<len; i+=1) {

                    var obj   = objects[i];
                    var id    = obj.id;
                    var shape = obj.getShape(e);

                    /**
                    * This bit saves the current pointer style if there isn't one already saved
                    */
                    var func = obj.Get('chart.events.click');
                    
                    if (!func && typeof(obj.onclick) == 'function') {
                        func = obj.onclick;
                    }
    
                    if (shape && typeof func == 'function') {
                        func(e, shape);
                        
                        /**
                        * If objects are layered on top of each other this return
                        * stops objects underneath from firing once the "top"
                        * objects user event has fired
                        */
                        return;
                    }
    
                    /**
                    * The property takes priority over this.
                    */
                    if (shape) {
    
                        var index = shape['object'].type == 'scatter' ? shape['index_adjusted'] : shape['index'];
        
                        if (typeof(index) == 'number' && obj['$' + index]) {
                            
                            var func = obj['$' + index].onclick;
                            
                            if (typeof(func) == 'function') {
                                
                                func(e, shape);
                                
                                /**
                                * If objects are layered on top of each other this return
                                * stops objects underneath from firing once the "top"
                                * objects user event has fired
                                */
                                return;
                            }
                        }
                    }
                    
                    /**
                    * This facilitates breaking out of the loop when a shape has been found - 
                    * ie the cursor is over a shape an upper chart
                    */
                    if (shape || (obj.overChartArea && obj.overChartArea(e)) ) {
                        break;
                    }
                }
            };
            obj.canvas.addEventListener('click', obj.canvas.rgraph_click_event_listener, false);
        }
    };




    /**
    * This function evaluates the various cursor settings and if there's one for pointer, changes it to that
    */
    //RGraph.evaluateCursor =
    RGraph.evaluateCursor =
    RGraph.EvaluateCursor = function (e)
    {
        var obj     = null;
        var mouseXY = RGraph.getMouseXY(e);
        var mouseX  = mouseXY[0];
        var mouseY  = mouseXY[1];
        var canvas  = e.target;

        /**
        * Tooltips cause the mouse pointer to change
        */
        var objects = RGraph.ObjectRegistry.getObjectsByCanvasID(canvas.id);
        
        for (var i=0,len=objects.length; i<len; i+=1) {
            if ((objects[i].getShape && objects[i].getShape(e)) || (objects[i].overChartArea && objects[i].overChartArea(e))) {
                var obj = objects[i];
                var id  = obj.id;
            }
        }

        if (!RGraph.is_null(obj)) {
            if (obj.getShape && obj.getShape(e)) {

                var shape = obj.getShape(e);

                if (obj.Get('chart.tooltips')) {

                    var text = RGraph.parseTooltipText(obj.Get('chart.tooltips'), shape['index']);
                    
                    if (!text && shape['object'].type == 'scatter' && shape['index_adjusted']) {
                        text = RGraph.parseTooltipText(obj.Get('chart.tooltips'), shape['index_adjusted']);
                    }

                    /**
                    * This essentially makes front charts "hide" the back charts
                    */
                    if (text) {
                        var pointer = true;
                    }
                }
            }

            /**
            * Now go through the key coords and see if it's over that.
            */
            if (!RGraph.is_null(obj) && obj.Get('chart.key.interactive')) {
                for (var j=0; j<obj.coords.key.length; ++j) {
                    if (mouseX > obj.coords.key[j][0] && mouseX < (obj.coords.key[j][0] + obj.coords.key[j][2]) && mouseY > obj.coords.key[j][1] && mouseY < (obj.coords.key[j][1] + obj.coords.key[j][3])) {
                        var pointer = true;
                    }
                }
            }
        }

        /**
        * It can be specified in the user mousemove event - remember it can now be specified in THREE ways
        */
        if (!RGraph.is_null(shape) && !RGraph.is_null(obj)) {

            if (!RGraph.is_null(obj.Get('chart.events.mousemove')) && typeof(obj.Get('chart.events.mousemove')) == 'function') {
                var str = (obj.Get('chart.events.mousemove')).toString();
                if (str.match(/pointer/) && str.match(/cursor/) && str.match(/style/)) {
                    var pointer = true;
                }
            }

            if (!RGraph.is_null(obj.onmousemove) && typeof(obj.onmousemove) == 'function') {
                var str = (obj.onmousemove).toString();
                if (str.match(/pointer/) && str.match(/cursor/) && str.match(/style/)) {
                    var pointer = true;
                }
            }
            
            var index = shape['object'].type == 'scatter' ? shape['index_adjusted'] : shape['index'];
            if (!RGraph.is_null(obj['$' + index]) && typeof(obj['$' + index].onmousemove) == 'function') {
                var str = (obj['$' + index].onmousemove).toString();
                if (str.match(/pointer/) && str.match(/cursor/) && str.match(/style/)) { 
                    var pointer = true;
                }
            }
        }

        /**
        * Is the chart resizable? Go through all the objects again
        */
        var objects = RGraph.ObjectRegistry.objects.byCanvasID;

        for (var i=0,len=objects.length; i<len; i+=1) {
            if (objects[i] && objects[i][1].Get('chart.resizable')) {
                var resizable = true;
            }
        }

        if (resizable && mouseX > (e.target.width - 32) && mouseY > (e.target.height - 16)) {
            pointer = true;
        }


        if (pointer) {
            e.target.style.cursor = 'pointer';
        } else if (e.target.style.cursor == 'pointer') {
            e.target.style.cursor = 'default';
        } else {
            e.target.style.cursor = null;
        }

        

        // =========================================================================
        // Resize cursor - check mouseis in bottom left corner and if it is change it
        // =========================================================================


        if (resizable && mouseX >= (e.target.width - 15) && mouseY >= (e.target.height - 15)) {
            e.target.style.cursor = 'move';
        }


        // =========================================================================
        // Interactive key
        // =========================================================================



        if (typeof mouse_over_key == 'boolean' && mouse_over_key) {
            e.target.style.cursor = 'pointer';
        }

        
        // =========================================================================
        // Gantt chart adjusting
        // =========================================================================


        if (obj && obj.type == 'gantt' && obj.Get('chart.adjustable')) {
            if (obj.getShape && obj.getShape(e)) {
                e.target.style.cursor = 'ew-resize';
            } else {
                e.target.style.cursor = 'default';
            }
        }

        
        // =========================================================================
        // Line chart adjusting
        // =========================================================================


        if (obj && obj.type == 'line' && obj.Get('chart.adjustable')) {
            if (obj.getShape && obj.getShape(e)) {
                e.target.style.cursor = 'ns-resize';
            } else {
                e.target.style.cursor = 'default';
            }
        }

        
        // =========================================================================
        // Annotatable
        // =========================================================================


        if (e.target.__object__ && e.target.__object__.Get('chart.annotatable')) {
            e.target.style.cursor = 'crosshair';
        }

        
        // =========================================================================
        // Drawing API link
        // =========================================================================


        if (obj && obj.type === 'drawing.text' && shape && typeof obj.Get('link') === 'string') {
            e.target.style.cursor = 'pointer';
        }
    };




    /**
    * This function handles the tooltip text being a string, function
    * 
    * @param mixed tooltip This could be a string or a function. If it's a function it's called and
    *                       the return value is used as the tooltip text
    * @param numbr idx The index of the tooltip.
    */
    RGraph.parseTooltipText = function (tooltips, idx)
    {
        // No tooltips
        if (!tooltips) {
            return null;
        }

        // Get the tooltip text
        if (typeof tooltips == 'function') {
            var text = tooltips(idx);

        // A single tooltip. Only supported by the Scatter chart
        } else if (typeof tooltips == 'string') {
            var text = tooltips;

        } else if (typeof tooltips == 'object' && typeof tooltips[idx] == 'function') {
            var text = tooltips[idx](idx);

        } else if (typeof tooltips[idx] == 'string' && tooltips[idx]) {
            var text = tooltips[idx];

        } else {
            var text = '';
        }

        if (text == 'undefined') {
            text = '';
        } else if (text == 'null') {
            text = '';
        }

        // Conditional in case the tooltip file isn't included
        return RGraph.getTooltipTextFromDIV ? RGraph.getTooltipTextFromDIV(text) : text;
    };




    /**
    * Draw crosshairs if enabled
    * 
    * @param object obj The graph object (from which we can get the context and canvas as required)
    */
    RGraph.drawCrosshairs =
    RGraph.DrawCrosshairs = function (e, obj)
    {
        var e            = RGraph.FixEventObject(e);
        var width        = obj.canvas.width;
        var height       = obj.canvas.height;
        var mouseXY      = RGraph.getMouseXY(e);
        var x            = mouseXY[0];
        var y            = mouseXY[1];
        var gutterLeft   = obj.gutterLeft;
        var gutterRight  = obj.gutterRight;
        var gutterTop    = obj.gutterTop;
        var gutterBottom = obj.gutterBottom;
        var Mathround    = Math.round;
        var prop         = obj.properties;
        var co           = obj.context;
        var ca           = obj.canvas;

        RGraph.RedrawCanvas(ca);

        if (   x >= gutterLeft
            && y >= gutterTop
            && x <= (width - gutterRight)
            && y <= (height - gutterBottom)
           ) {

            var linewidth = prop['chart.crosshairs.linewidth'] ? prop['chart.crosshairs.linewidth'] : 1;
            co.lineWidth = linewidth ? linewidth : 1;

            co.beginPath();
            co.strokeStyle = prop['chart.crosshairs.color'];





            /**
            * The chart.crosshairs.snap option
            */
            if (prop['chart.crosshairs.snap']) {
            
                // Linear search for the closest point
                var point = null;
                var dist  = null;
                var len   = null;
                
                if (obj.type == 'line') {
            
                    for (var i=0; i<obj.coords.length; ++i) {
                    
                        var length = RGraph.getHypLength(obj.coords[i][0], obj.coords[i][1], x, y);
            
                        // Check the mouse X coordinate
                        if (typeof dist != 'number' || length < dist) {
                            var point = i;
                            var dist = length;
                        }
                    }
                
                    x = obj.coords[point][0];
                    y = obj.coords[point][1];
                    
                    // Get the dataset
                    for (var dataset=0; dataset<obj.coords2.length; ++dataset) {
                        for (var point=0; point<obj.coords2[dataset].length; ++point) {
                            if (obj.coords2[dataset][point][0] == x && obj.coords2[dataset][point][1] == y) {
                                ca.__crosshairs_snap_dataset__ = dataset;
                                ca.__crosshairs_snap_point__   = point;
                            }
                        }
                    }

                } else {
            
                    for (var i=0; i<obj.coords.length; ++i) {
                        for (var j=0; j<obj.coords[i].length; ++j) {
                            
                            // Check the mouse X coordinate
                            var len = RGraph.getHypLength(obj.coords[i][j][0], obj.coords[i][j][1], x, y);
            
                            if (typeof(dist) != 'number' || len < dist) {
            
                                var dataset = i;
                                var point   = j;
                                var dist   = len;
                            }
                        }
            
                    }
                    ca.__crosshairs_snap_dataset__ = dataset;
                    ca.__crosshairs_snap_point__   = point;

            
                    x = obj.coords[dataset][point][0];
                    y = obj.coords[dataset][point][1];
                }
            }






            // Draw a top vertical line
            if (prop['chart.crosshairs.vline']) {
                co.moveTo(Mathround(x), Mathround(gutterTop));
                co.lineTo(Mathround(x), Mathround(height - gutterBottom));
            }

            // Draw a horizontal line
            if (prop['chart.crosshairs.hline']) {
                co.moveTo(Mathround(gutterLeft), Mathround(y));
                co.lineTo(Mathround(width - gutterRight), Mathround(y));
            }

            co.stroke();
            
            
            /**
            * Need to show the coords?
            */
            if (obj.type == 'scatter' && prop['chart.crosshairs.coords']) {

                var xCoord = (((x - gutterLeft) / (width - gutterLeft - gutterRight)) * (prop['chart.xmax'] - prop['chart.xmin'])) + prop['chart.xmin'];
                    xCoord = xCoord.toFixed(prop['chart.scale.decimals']);
                var yCoord = obj.max - (((y - prop['chart.gutter.top']) / (height - gutterTop - gutterBottom)) * obj.max);

                if (obj.type == 'scatter' && obj.properties['chart.xaxispos'] == 'center') {
                    yCoord = (yCoord - (obj.max / 2)) * 2;
                }

                yCoord = yCoord.toFixed(prop['chart.scale.decimals']);

                var div      = RGraph.Registry.Get('chart.coordinates.coords.div');
                var mouseXY  = RGraph.getMouseXY(e);
                var canvasXY = RGraph.getCanvasXY(ca);
                
                if (!div) {
                    var div = document.createElement('DIV');
                    div.__object__     = obj;
                    div.style.position = 'absolute';
                    div.style.backgroundColor = 'white';
                    div.style.border = '1px solid black';
                    div.style.fontFamily = 'Arial, Verdana, sans-serif';
                    div.style.fontSize = '10pt'
                    div.style.padding = '2px';
                    div.style.opacity = 1;
                    div.style.WebkitBorderRadius = '3px';
                    div.style.borderRadius = '3px';
                    div.style.MozBorderRadius = '3px';
                    document.body.appendChild(div);
                    
                    RGraph.Registry.Set('chart.coordinates.coords.div', div);
                }

                // Convert the X/Y pixel coords to correspond to the scale
                div.style.opacity = 1;
                div.style.display = 'inline';

                if (!prop['chart.crosshairs.coords.fixed']) {
                    div.style.left = Math.max(2, (e.pageX - div.offsetWidth - 3)) + 'px';
                    div.style.top = Math.max(2, (e.pageY - div.offsetHeight - 3))  + 'px';
                } else {
                    div.style.left = canvasXY[0] + gutterLeft + 3 + 'px';
                    div.style.top  = canvasXY[1] + gutterTop + 3 + 'px';
                }

                div.innerHTML = '<span style="color: #666">' + prop['chart.crosshairs.coords.labels.x'] + ':</span> ' + xCoord + '<br><span style="color: #666">' + prop['chart.crosshairs.coords.labels.y'] + ':</span> ' + yCoord;

                obj.canvas.addEventListener('mouseout', RGraph.HideCrosshairCoords, false);

                ca.__crosshairs_labels__ = div;
                ca.__crosshairs_x__ = xCoord;
                ca.__crosshairs_y__ = yCoord;

            } else if (prop['chart.crosshairs.coords']) {
                alert('[RGRAPH] Showing crosshair coordinates is only supported on the Scatter chart');
            }

            /**
            * Fire the oncrosshairs custom event
            */
            RGraph.FireCustomEvent(obj, 'oncrosshairs');

        } else {
            RGraph.HideCrosshairCoords();
        }
    };

// End module pattern
})(window, document);
// version: 2014-03-28


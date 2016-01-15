/* =============================================================
 * bootstrap-combobox.js v1.1.5
 * =============================================================
 * Copyright 2012 Daniel Farrell
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ============================================================ */

!function ($) {

    "use strict";

    /* COMBOBOX PUBLIC CLASS DEFINITION
     * ================================ */
    var Combobox = function (element, options) {
        this.loading = true;
        this.options = $.extend({}, $.fn.combobox.defaults, options);

        this.islookup = false;
        if ($(element).hasClass('lookup')) {
            this.islookup = true;
            this.options.template = '';
        }

        this.options.pageSize = this.options.pageSize || 30000;
        this.$source = $(element);
        this.$container = this.setup();

        if (!this.islookup) {
            this.$element = this.$container.find('input[type=text]');
            this.$target = this.$container.find('input[type=hidden]');
            this.$button = this.$container.find('.dropdown-toggle');
        } else {
            this.$element = this.$source.prev().find('input[type=text]');
            this.$target = this.$source.prev().find('input[type=hidden]');
        }

        this.$menu = $(this.options.menu).appendTo('body');
        this.matcher = this.options.matcher || this.matcher;
        this.sorter = this.options.sorter || this.sorter;
        this.highlighter = this.options.highlighter || this.highlighter;
        this.shown = false;
        this.selected = false;
        this.refresh();
        this.transferAttributes();
        this.listen();
        this.loading = false;
    };

    Combobox.prototype = {

        constructor: Combobox

    , setup: function () {
        var combobox = $(this.options.template);
        this.$source.before(combobox);
        this.$source.hide();
        return combobox;
    },

        disable: function () {
            this.$element.prop('disabled', true);
            this.$button.attr('disabled', true);
            this.disabled = true;
            this.$container.addClass('combobox-disabled');
        },

        enable: function () {
            this.$element.prop('disabled', false);
            this.$button.attr('disabled', false);
            this.disabled = false;
            this.$container.removeClass('combobox-disabled');
        },

        parse: function () {
            var that = this
              , map = {}
              , source = []
              , selected = false
              , selectedValue = '';
            this.$source.find('option').each(function () {
                var option = $(this);
                if (option.val() === '') {
                    that.options.placeholder = option.text();
                    return;
                }
                var description = option.text().replace(/\s+/g, ' ');
                map[description] = option.val();
                source.push(description);
                if (option.prop('selected')) {
                    selected = description;
                    selectedValue = option.val();
                }
            })
            this.map = map;
            if (selected) {
                this.$element.val(selected);
                this.$target.val(selectedValue);
                //this.$container.addClass('combobox-selected');
                this.selected = true;
            }
            return source.sort();
        }


    , transferAttributes: function () {
        this.options.placeholder = this.$source.attr('data-placeholder') || this.options.placeholder;
        this.$element.attr('placeholder', this.options.placeholder);
        this.$target.prop('name', this.$source.prop('name'));
        this.$target.val(this.$source.val());
        this.$source.removeAttr('name');  // Remove from source otherwise form will pass parameter twice.
        this.$element.attr('required', this.$source.attr('required'));
        this.$element.attr('rel', this.$source.attr('rel'));
        this.$element.attr('title', this.$source.attr('title'));
        this.$element.attr('class', this.$source.attr('class'));
        this.$element.attr('tabindex', this.$source.attr('tabindex'));

        this.$source.removeAttr('tabindex');
        if (this.$source.attr('disabled') !== undefined) {
            this.disable();
        }
    }

    , select: function () {

        var val = this.$menu.find('.active').attr('data-value');
        this.$element.val(this.updater(val)).trigger('change');
        this.$target.val(this.map[val]).trigger('change');
        this.$source.val(this.map[val]).trigger('change');
        //sthis.$container.addClass('combobox-selected');
        this.selected = true;
        return this.hide();
    }

    , updater: function (item) {
        return item;
    },

        determineTop: function (position) {

            var maxHeight = this.$menu.css('max-height');
            var height = this.$menu.height();
            if (maxHeight) {
                maxHeight = maxHeight.replace('px', '');
            }
            if (maxHeight < height) {
                height = maxHeight;
            }

            //SWWEB-1133 if the combobox is near the bootom of the screen, place the dropdown above the input
            if ((this.$element.offset().top + height > document.body.scrollHeight) || (this.$element.offset().top + height + 50 - $(window).scrollTop() > $(window).height())) {
                //https://controltechnologysolutions.atlassian.net/browse/HAP-847
                //if the menu would cause a scrollbar expansion open it on top instead
                //12 stands for padding, margin, etc
                return -height - 15;
            }

            return position.top + position.height;
        }


    , show: function () {
        var pos = $.extend({}, this.$element.position(), {
            height: this.$element[0].offsetHeight
        });
        var top = this.determineTop(pos);


        this.$menu
          .insertAfter(this.$element)
          .css({
              top: top
          , left: pos.left,
              width: pos.width
          })
          .show();
        $('.dropdown-menu').on('mousedown', $.proxy(this.scrollSafety, this));
        $('.dropdown-menu').on('scroll', $.proxy(this.paginate, this));
        this.shown = true;
        $(document).trigger("sw_autocompleteselected", this.$source.data("associationkey"));
        return this;
    }

    , hide: function () {
        this.$menu.hide();
        $('.dropdown-menu').off('mousedown', $.proxy(this.scrollSafety, this));
        $('.dropdown-menu').off('scroll', $.proxy(this.paginate, this));
        this.$element.on('blur', $.proxy(this.blur, this));
        this.shown = false;
        if (this.$target.val() == '') {
            this.triggerChange();
        }
        return this;
    }

    , lookup: function (event, initialPage, options) {
        initialPage = initialPage || 0;

        //get options
        var lookupall = false;
        if (typeof options != 'undefined') {
            if (typeof options.lookupall != 'undefined') {
                lookupall = options.lookupall;
            }
        }

        var val = this.$element.val();
        if (val.length >= this.options.minLength || val == '') {

            if (!lookupall) {
                //filter list based on user input
                this.query = this.$element.val();
            } else {
                //don't filter the list
                this.query = '';
            }
            this.currentPage = initialPage;
            if (this.idxShown == undefined) {
                this.idxShown = [];
            }

            return this.process(this.source, initialPage);
        }
    }

    , process: function (items, initialPage) {
        var that = this;
        var foundItems = [];

        var pageSize = this.options.pageSize;
        var pageLimit = pageSize;
        if (items.length < pageLimit) {
            pageLimit = items.length;
        }

        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            var idxNotPresent = this.currentPage == 0 || this.idxShown.indexOf(i) == -1;
            if (that.matcher(item) && idxNotPresent && foundItems.length < pageLimit) {
                foundItems.push(item);
                this.idxShown.push(i);
            }
        }

        foundItems = this.sorter(foundItems);

        if (!foundItems.length) {
            if (initialPage == 0) {

                //don't refresh (clear the input), instead highlight in red
                //this.refresh();
                this.$element.addClass('not-found');

                //if we're not on current page, let's avoid calling hide whenever the scroll reaches the end
                this.$target.val(this.previousTarget);
                this.$source.val(this.previousTarget);
                // this.$element.val(this.previousTarget);

                if (this.shown) {
                    return this.hide();
                }
                return this;
            }
            return this;
        } else {
            this.$element.removeClass('not-found');
        }

        var appendItems = initialPage != 0;
        this.render(foundItems.slice(0, foundItems.length), appendItems);
        if (initialPage == 0) {
            //if we�re on the first page, it means we oughtta show the menu, otherwise it�s stil opened and the user is only scrolling it
            return this.show();
        }
        return this;
    }

    , matcher: function (item) {
        return ~item.toLowerCase().replace(/\s+/g, ' ').indexOf(this.query.toLowerCase());
    }

    , sorter: function (items) {
        var beginswith = []
          , caseSensitive = []
          , caseInsensitive = []
          , item;


        while ((item = items.shift()) != null) {
            if (item === "") {
                continue;
            }

            item = item.replace(/\s+/g, ' ');
            if (!item.toLowerCase().indexOf(this.query.toLowerCase())) {
                beginswith.push(item);
            }
            else if (~item.indexOf(this.query)) {
                caseSensitive.push(item);
            } else {
                caseInsensitive.push(item);
            }
        }

        return beginswith.concat(caseSensitive, caseInsensitive);
    }

    , highlighter: function (item) {
        var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
        return item.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
            return '<strong>' + match + '</strong>';
        })
    }

    , render: function (items, appendItems) {
        var that = this;

        items = $(items).map(function (i, item) {
            i = $(that.options.item).attr('data-value', item);
            i.find('a').html(that.highlighter(item));
            return i[0];
        });
        if (appendItems) {
            //if we are not on first page then we need to append the items to the existing html element instead of creating one
            this.$menu.append(items);
        } else {
            items.first().addClass('active');
            this.$menu.html(items);
        }

        return this;
    }

    , next: function (event) {

        var active = this.$menu.find('.active').removeClass('active')
          , next = active.next();

        if (!next.length) {
            next = $(this.$menu.find('li')[0]);
        }

        next.addClass('active');
        return next;
    }

    , prev: function (event) {
        var active = this.$menu.find('.active').removeClass('active')
          , prev = active.prev();

        if (!prev.length) {
            prev = this.$menu.find('li').last();
        }

        prev.addClass('active');
        return prev;
    }

    , toggle: function () {
        if (this.disabled) {
            return;
        }
        this.idxShown = [];
        //if (this.$container.hasClass('combobox-selected')) {
        //    this.clearTarget();
        //    this.triggerChange();
        //    this.clearElement();
        //} else {
        if (this.shown) {
            this.hide();
        } else {
            //don't clear the contents, just show the whole list
            this.lookup(null, null, { lookupall: true });
        }
        //}
    },

        scrollSafety: function (e) {
            if (e.target.tagName == 'UL') {
                this.$element.off('blur');
            }
        },

        paginate: function (e) {
            var target = e.target;
            if (target.offsetHeight + target.scrollTop >= target.scrollHeight) {
                this.lookup(e, this.currentPage + 1);
            }

        },

        clearElement: function () {
            //don't autofocus until the combobox loads
            this.$element.val('');
            //            if (this.loading) {
            ////                this.$element.val('');
            //            } else {
            //                .focus();
            //            }
        }

    , clearTarget: function () {
        this.$source.val('');
        this.$target.val('');
        //this.$container.removeClass('combobox-selected');
        this.selected = false;
    }

    , triggerChange: function () {
        this.$source.trigger('change');
    }

    , refresh: function (newValue) {
        this.source = this.parse();
        this.options.items = this.source.length;

        if (newValue !== undefined) {
            //if null we should set it
            this.$element.val(newValue);
        } else if (this.source && this.source.length === 1) {
            if (this.selected) {
                this.$element.val(this.source[0]);
            }
        }
        var val = this.$element.val();
        if (!this.source || val.trim() === "") {
            this.clearElement();
        }
    }

    , listen: function () {
        this.$element
            .on('focus', $.proxy(this.focus, this))
            .on('blur', $.proxy(this.blur, this))
            .on('keypress', $.proxy(this.keypress, this))
            .on('keyup', $.proxy(this.keyup, this))

            //display the dropdown when clicked
            .on('click', $.proxy(this.toggle, this));

        if (this.eventSupported('keydown')) {
            this.$element.on('keydown', $.proxy(this.keydown, this));
        }

        this.$menu
          .on('click', $.proxy(this.click, this))
          .on('mouseenter', $.proxy(function () { this.ulMousedOver = true; }, this))
          .on('mouseleave', $.proxy(function () {
              this.ulMousedOver = false;
              if (!this.mousedover && this.shown && !this.$element.is(":focus")) {
                  var that = this;
                  setTimeout(function () { that.hide(); }, 200);
              }
          }, this))
            .on('mouseenter', 'li', $.proxy(this.mouseenter, this))
            .on('mouseleave', 'li', $.proxy(this.mouseleave, this));;

        if (!this.islookup) {
            this.$button
                //clear the contents then display the dropdown
                .on('click', $.proxy(this.toggle, this));
        }
    }

    , eventSupported: function (eventName) {
        var isSupported = eventName in this.$element;
        if (!isSupported) {
            this.$element.setAttribute(eventName, 'return;');
            isSupported = typeof this.$element[eventName] === 'function';
        }
        return isSupported;
    }

    , move: function (e) {
        if (!this.shown) {
            return;
        }
        switch (e.keyCode) {
            case 9: // tab
            case 13: // enter
            case 27: // escape
                e.preventDefault();
                break;
            case 38: // up arrow
                e.preventDefault();
                var element = this.prev();
                this.$menu.scrollTop(element.position().top + this.$menu.scrollTop());
                break;

            case 40: // down arrow
                e.preventDefault();
                var element = this.next();
                this.$menu.scrollTop(element.position().top + this.$menu.scrollTop());
                break;
        }

        e.stopPropagation();
    }

    , keydown: function (e) {
        this.suppressKeyPressRepeat = ~$.inArray(e.keyCode, [40, 38, 9, 13, 27]);
        this.move(e);
    }

    , keypress: function (e) {
        if (this.suppressKeyPressRepeat) { return; }
        this.move(e);
    }

    , keyup: function (e) {
        switch (e.keyCode) {
            case 40: // down arrow
            case 39: // right arrow
            case 38: // up arrow
            case 37: // left arrow
            case 36: // home
            case 35: // end
            case 16: // shift
            case 17: // ctrl
            case 18: // alt
                break;
            case 8:
            case 46:
                if (this.$element.val() == '') {
                    this.clearTarget();
                    this.triggerChange();
                }
                break;
            case 9: // tab
            case 13: // enter
                this.select();
                e.stopPropagation();
                e.preventDefault();
                return;
            case 27: // escape
                if (!this.shown) { return; }
                this.hide();
                break;
            case 67:
                if ((e.ctrlKey || e.metaKey)) {
                    //avoid ctrl +C
                    break;
                }

            default:
                if (this.$target.val() != '') {
                    this.previousTarget = this.$target.val()
                }
                this.clearTarget();
        }

        //always display the dropdown
        this.lookup();

        e.stopPropagation();
        e.preventDefault();
    }

    , focus: function (e) {
        this.focused = true;
    },

        setFocus: function (e) {
            this.$element.focus();
            this.focused = true;
        }

    , blur: function (e) {
        var that = this;
        this.focused = false;
        var val = this.$element.val();
        if (!this.selected && val != '') {
            this.$element.val('');
            this.$source.val('').trigger('change');
            this.$target.val('').trigger('change');
        }
        if (!this.ulMousedOver && !this.mousedover && this.shown) { setTimeout(function () { that.hide(); }, 200); }
    }

    , click: function (e) {
        e.stopPropagation();
        e.preventDefault();
        this.select();
        //        this.$element.focus();
    }

    , mouseenter: function (e) {
        this.mousedover = true;
        this.$menu.find('.active').removeClass('active');
        $(e.currentTarget).addClass('active');
    }

    , mouseleave: function (e) {
        this.mousedover = false;
    }
    };

    /* COMBOBOX PLUGIN DEFINITION
     * =========================== */
    $.fn.combobox = function (option) {
        return this.each(function () {
            var $this = $(this)
              , data = $this.data('combobox')
              , options = typeof option == 'object' && option;
            if (!data) { $this.data('combobox', (data = new Combobox(this, options))); }
            if (typeof option == 'string') { data[option](); }
        });
    };

    $.fn.combobox.defaults = {
        template: '<div class="combobox-container input-group" >' +
            '<input type="hidden" /><input type="text" autocomplete="off"/>' +
            '<span class="input-group-addon dropdown-toggle" data-dropdown="dropdown">' +
            '<span class="caret"/>' +
            '</span></div>'
    , menu: '<ul class="typeahead typeahead-long dropdown-menu" style="width:94%"></ul>'
    , item: '<li><a href="#"></a></li>'
    , minLength: 1
    };

    $.fn.combobox.Constructor = Combobox;

}(window.jQuery);
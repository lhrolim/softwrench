$(function () {

    //ensure that english is the current locale for moment.js
    moment.locale('en');

    //show or hide the menu when the expand button is clicked
    $('.menu-expand').click(function () {
        $(this).toggleClass('menu-open');
    });
});

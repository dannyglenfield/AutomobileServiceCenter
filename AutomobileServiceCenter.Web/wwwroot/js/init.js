(function($){
  $(function(){

      $('.sidenav').sidenav();
      $('.parallax').parallax();

      $('.collapsible').collapsible();
      $('.collapsible.expandable').collapsible({
        accordion: false
    });

  }); // end of document ready
})(jQuery); // end of jQuery name space

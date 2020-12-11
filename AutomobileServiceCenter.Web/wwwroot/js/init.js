(function($){
  $(function(){

      $('.sidenav').sidenav();
      $('.parallax').parallax();

      $('.collapsible').collapsible();
      $('.collapsible.expandable').collapsible({
        accordion: false
      });

      // Prevent browser back and forward buttons.
      if (window.history && window.history.pushState) {
          window.history.pushState('forward', '', window.location.href);

          $(window).on('popstate', function (e) {
              window.history.pushState('forward', '', window.location.href);
              e.preventDefault();
          });
      }
      // Prevent right-click on entire window
      $(document).ready(function () {
          $(window).on("contextmenu", function () {
              return false;
          });
      });

  }); // end of document ready
})(jQuery); // end of jQuery name space

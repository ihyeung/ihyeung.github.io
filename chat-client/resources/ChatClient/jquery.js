
$(document).ready(function() {
    $( "#enter" ).click(function() {
        $( "#enterchatroom" ).fadeOut( "slow", "linear" );
        $( ".chat" ).fadeIn( "slow", "linear" );
      });
      $( "#login" ).click(function() {
          $( "#loginstatus" ).fadeIn("slow","linear");
          $( "#loginstatus" ).fadeOut("slow","linear");
          $( "#enterchatroom" ).fadeIn("slow","linear");
      });


      });
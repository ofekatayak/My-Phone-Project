console.log("Loaded!")
    document.getElementById("cartIcon").addEventListener("click", function () {
            var myDiv = document.getElementById('cartSideScreen');
        if (myDiv.style.display === "none") {
            myDiv.style.display = "block"; // Change display to block
        } else {
            myDiv.style.display = "none"; // Change display to none
        }

    });
    // hide cart side screen
    document.getElementById("closeCartModalBtn").addEventListener("click", function () {
        var cartSide = document.getElementById("cartSideScreen");
        cartSide.style.display = "none";
    });

    // show filter side screen
    document.getElementById("filterBttn").addEventListener("click", function () {
         var myDiv = document.getElementById('filterSideScreen');
        if (myDiv.style.display === "none")
        {
            myDiv.style.display = "block";
        } else {
            myDiv.style.display = "none";
        }

    });
    // hide filter side screen
    document.getElementById("closeFilterModalBtn").addEventListener("click", function () {
         var filterSide = document.getElementById("filterSideScreen");
        filterSide.style.display = "none";
    });

    // save categoryName
    var categotyName = 'Apple';

    function saveElementName(element) {
         var elementName = element.innerText;
        categotyName = elementName;
        console.log("Element name:", categotyName);
    }



    // hide Pay side screen
    document.getElementById("closePayModalBtn").addEventListener("click", function () {
        var payDiv = document.getElementById('PaySideScreen');
        var cartDiv = document.getElementById('cartSideScreen');
        payDiv.style.display = "none";
        cartDiv.style.display = "block";
        console.log("close!")
    });



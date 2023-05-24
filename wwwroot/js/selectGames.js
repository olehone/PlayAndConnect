function getGameOptions() {
    var gameName = $('#gameNameInput').val();
    
    $.ajax({
      url: '/Home/LHssdhsaoidhgaoeghaiwodiejg',
      type: 'POST',
      data: { gameName: gameName },
      success: function(result) {
        populateGameOptions(result);
      },
      error: function(xhr, status, error) {
        console.log('Error:', error); // Виводимо помилку в консоль
      }
    });
  }
  
  function populateGameOptions(options) {
    var gameOptionsSelect = $('#gameOptions');
    gameOptionsSelect.empty();
  
    // Додавання варіантів у випадаючий список
    for (var i = 0; i < options.length; i++) {
      var option = options[i];
      var optionElement = $('<option>').text(option);
      gameOptionsSelect.append(optionElement);
    }
  }
  
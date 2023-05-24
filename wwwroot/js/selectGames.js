function getGameOptions() {
  var gameName = $('#gameNameInput').val().trim();
  if (gameName.length > 0&& gameName.length<30) {
    $.ajax({
      url: '/Home/GetGameOptions',
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
}

function populateGameOptions(options) {
  console.log(options);
  var gameOptionsSelect = $('#gameOptions');
  gameOptionsSelect.empty();

  // Додавання варіантів у випадаючий список
  for (var i = 0; i < options.length; i++) {
    var option = options[i];
    var optionElement = $('<option>').text(option.title).val(option.id);
    gameOptionsSelect.append(optionElement);
  }

  // При виборі гри автоматично записуємо її назву в рядок пошуку
  gameOptionsSelect.on('change', function() {
    var selectedGame = $(this).find('option:selected').text();
    $('#gameNameInput').val(selectedGame);
  });
}


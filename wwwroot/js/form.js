const express = require("express");
const router = express.Router();
const mysql = require("mysql2/promise");

// Подключение к базе данных
const pool = mysql.createPool({
  host: "localhost",
  user: "root",
  password: "password",
  database: "mydb",
});

router.post("/register", async (req, res) => {
  const { email, username, password } = req.body;

  // Проверка наличия обязательных полей
  if (!email || !username || !password) {
    return res.status(400).json({ message: "Please fill all the fields" });
  }

  try {
    // Выполнение запроса на добавление пользователя в базу данных
    const [rows, fields] = await pool.query(
      "INSERT INTO users (email, username, password) VALUES (?, ?, ?)",
      [email, username, password]
    );

    return res.status(201).json({ message: "User registered successfully" });
  } catch (error) {
    console.error(error);
    return res.status(500).json({ message: "Internal Server Error" });
  }
});

module.exports = router;
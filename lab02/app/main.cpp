#include <iostream>
#include <string>
#include <random>
#include "httplib.h"

using namespace std;

/* ---------------- Wheel ---------------- */

class Wheel {

public:
    int spin() {
        static random_device rd;
        static mt19937 gen(rd());
        uniform_int_distribution<> dist(0, 36);

        return dist(gen);
    }

    string getColor(int number) {

        if (number == 0)
            return "green";

        if (number % 2 == 0)
            return "black";

        return "red";
    }
};

/* ---------------- BalanceManager ---------------- */

class BalanceManager {

private:
    double balance = 100000;

public:

    bool check(double amount) {
        return balance >= amount;
    }

    void deduct(double amount) {
        balance -= amount;
    }

    void add(double amount) {
        balance += amount;
    }

    double getBalance() {
        return balance;
    }
};

/* ---------------- PayoutSystem ---------------- */

class PayoutSystem {

private:
    double taxRate = 0.13;

public:

    double calculate(string betColor, double betAmount, string winColor) {

        if (betColor != winColor)
            return 0;

        double win;

        if (winColor == "green")
            win = betAmount * 17;
        else
            win = betAmount * 2;

        return win * (1 - taxRate);
    }
};

/* ---------------- CasinoFacade ---------------- */

class CasinoFacade {

private:
    BalanceManager balance;
    Wheel wheel;
    PayoutSystem payout;

public:

    string play(string color, double amount) {

        if (!balance.check(amount))
            return "Not enough balance";

        balance.deduct(amount);

        int number = wheel.spin();
        string resultColor = wheel.getColor(number);

        double finalWin = payout.calculate(color, amount, resultColor);

        if (finalWin > 0)
            balance.add(finalWin);

        return
            "Number: " + to_string(number) +
            " Color: " + resultColor +
            " Win: " + to_string(finalWin) +
            " Balance: " + to_string(balance.getBalance());
    }
};

/* ---------------- MAIN ---------------- */

int main() {

    CasinoFacade casino;

    httplib::Server server;

    // ----------------- ОТДАЁМ index.html -----------------
    server.Get("/", [](const httplib::Request&, httplib::Response& res) {
        std::ifstream file("index.html");       // открываем файл
        std::stringstream buffer;
        buffer << file.rdbuf();                 // читаем содержимое
        res.set_content(buffer.str(), "text/html"); // отправляем как HTML
    });

    // ----------------- Обработка ставок -----------------
    server.Post("/bet", [&](const httplib::Request& req, httplib::Response& res) {
        string color = req.get_param_value("color");
        double amount = stod(req.get_param_value("amount"));

        string result = casino.play(color, amount);

        res.set_content(result, "text/plain");
    });

    cout << "Server started: http://localhost:8080\n";

    server.listen("localhost", 8080);
}
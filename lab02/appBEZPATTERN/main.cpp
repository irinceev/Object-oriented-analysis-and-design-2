#include <iostream>
#include <string>
#include <random>
#include <fstream>
#include <sstream>
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

/* ---------------- MAIN ---------------- */

int main() {

    BalanceManager balance;
    Wheel wheel;
    PayoutSystem payout;

    httplib::Server server;

    server.Get("/", [](const httplib::Request&, httplib::Response& res) {
        ifstream file("index.html");
        stringstream buffer;
        buffer << file.rdbuf();
        res.set_content(buffer.str(), "text/html");
    });

    server.Post("/bet", [&](const httplib::Request& req, httplib::Response& res) {

        string color = req.get_param_value("color");
        double amount = stod(req.get_param_value("amount"));

        if (!balance.check(amount)) {
            res.set_content("Not enough balance", "text/plain");
            return;
        }

        balance.deduct(amount);

        int number = wheel.spin();
        string resultColor = wheel.getColor(number);

        double finalWin = payout.calculate(color, amount, resultColor);

        if (finalWin > 0)
            balance.add(finalWin);

        string result =
            "Number: " + to_string(number) +
            " Color: " + resultColor +
            " Win: " + to_string(finalWin) +
            " Balance: " + to_string(balance.getBalance());

        res.set_content(result, "text/plain");
    });

    cout << "Server started: http://localhost:8080\n";

    server.listen("localhost", 8080);
}

const express = require("express");
const app = express();
app.use(express.json());

app.get("/data", (req, res) => {
  res.json({
    stateId:5,
    stateName:"Sample State",
  });
});

app.post("/click", (req, res) => {
  console.log("Received click:", req.body);
  res.json({ status: "ok" });
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});

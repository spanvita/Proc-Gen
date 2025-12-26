
const express = require("express");
const app = express();

app.get("/data", (req, res) => {
  res.json({
    stateId:5,
    stateName:"Sample State",
  });
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});

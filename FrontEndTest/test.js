

const getAndPrintDepts = () => {
  fetch("https://localhost:5001/api/Department")
    .then(depts => depts.json())
    .then(parsedDepts => {
      parsedDepts.forEach(dept => {
        document.querySelector("#output").innerHTML += `<div>
                <p>${dept.name}</p>
                <p>${dept.budget}</p>
                            </div>`;
      });
    });
};

getAndPrintDepts();
const path = 'https://localhost:7284/';

window.addEventListener('load', function () {
    this.document.querySelector('#password-sign-in-btn')
        .addEventListener('click', loginViaPassword);
});

function loginViaPassword() {
    console.log('loginViaPassword');
    var password = document.querySelector('#password-sign-in-input').value;
    var data = new Object();
    data['password'] = password;
    data = JSON.stringify(data);
    postRequest(
        path + 'api/Auth/passwordLogin/',
        data
    )
        .then(response => {
            if (response.ok) {
                window.location.replace(path + 'pages/courses.html');
            }
            else {
                alert('Incorrect password.')
            }
        })
        .catch(error => () => { console.error(error), alert('Incorrect passwrod.') })
}

function postRequest(url, data) {
    return fetch(url,
        {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: data
        }
    ).then(response => response);
}
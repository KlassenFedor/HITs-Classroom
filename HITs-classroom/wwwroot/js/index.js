const path = 'https://localhost:7284/';

window.addEventListener('load', function () {
    const logoutBtn = this.document.querySelector('#logout-button');
    logoutBtn.addEventListener('click', logout);
});

function logout() {
    postRequestWithoutResponseBody(
        path + 'api/Auth/logout'
    )
        .then(response => {
            if (response.status == 200) {
                alert('Successfully logged out.');
            } else {
                alert('Failed to log out.');
            }
        })
        .catch(error => console.error(error));
}

function postRequestWithoutResponseBody(url, data) {
    return fetch(url,
        {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: data
        }
    );
}

function post(url, data) {
    return fetch(url,
        {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: data
        }
    ).then(response => response.json());
}

function handleCredentialResponse(response) {
    console.log("Encoded JWT ID token: " + response.credential);
    var data = new Object();
    data['token'] = response.credential;
    data = JSON.stringify(data);
    postRequestWithoutResponseBody(
        path + 'api/Auth/Login',
        data
    )
        .then(response => {
            if (response.status == 200) {
                alert('Successfully logged in.');
            }
            else {
                alert('Failed to log in.');
            }
        })
        .catch(error => console.error(error));
}

window.onload = function () {
    google.accounts.id.initialize({
        client_id: "661958257383-s7poh0ppg147d1e3icckoc08v5g3mg3i.apps.googleusercontent.com",
        callback: handleCredentialResponse
    });
    google.accounts.id.renderButton(
        document.getElementById("buttonDiv"),
        { theme: "outline", size: "large" }
    );
}
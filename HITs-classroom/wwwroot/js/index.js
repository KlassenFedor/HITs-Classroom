const path = 'https://localhost:7284/';

window.addEventListener('load', function () {
    const logoutBtn = this.document.querySelector('#logout-button');
    logoutBtn.addEventListener('click', logout);

    const loginA = this.document.querySelector('#google-login');
    loginA.setAttribute('href', 'https://accounts.google.com/o/oauth2/auth?' +
                                 'redirect_uri=http://localhost/google-auth&' +
                                 'response_type=code&' +
                                 'client_id=661958257383-e3ftp6ndnksr4hae0cvg8nv83me6vurk.apps.googleusercontent.com&' +
                                 'scope=https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/classroom.profile.emails https://www.googleapis.com/auth/classroom.courses https://www.googleapis.com/auth/classroom.rosters'
    );
});

//function login() {
//    postRequestWithoutResponseBody(
//        path + 'account/google-login'
//    )
//        .then(response => {
//            if (response.status == 200) {
//                alert('Successfully logged in.');
//            } else {
//                alert('Failed to log in.');
//            }
//        })
//        .catch(error => console.error(error));
//}

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
    console.log(parseJwt(response.credential));
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

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
};
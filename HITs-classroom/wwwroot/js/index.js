const path = 'https://localhost:7284/';

window.addEventListener('load', function () {
    const createBtn = document.querySelector('#createCourseSubmitButton');
    if (createBtn) {
        createBtn.addEventListener("click", createCourse);
    }
    const syncButton = this.document.querySelector('#sync-courses-button');
    syncButton.addEventListener('click', syncCourses)
});

function createCourse() {
    var data = new Object();
    for (const pair of new FormData(document.querySelector('#createCourseForm'))) {
        if (pair[1] != null && pair[1] != '') {
            data[pair[0]] = pair[1];
        }
    }
    data = JSON.stringify(data);
    post(
        path + 'api/Courses/create',
        data
    )
        .then(response => console.log(response))
        .catch(error => console.error(error))
}

function syncCourses() {
    post(
        path + 'api/Courses/synchronize'
    )
        .then(response => console.log(response))
        .catch(error => console.error(error))
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
    post(
        path + 'api/Auth/Login',
        data
    )
        .then(response => console.log(response))
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

////
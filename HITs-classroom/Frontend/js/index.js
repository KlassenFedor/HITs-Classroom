const path = 'https://localhost:7284/';

window.addEventListener('load', function () {
    const createBtn = document.querySelector('#createCourseSubmitButton');
    if (createBtn) {
        createBtn.addEventListener("click", createCourse);
    }
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
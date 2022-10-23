const path = 'https://localhost:7284/';

window.addEventListener('load', function () {

    this.document.querySelector('#logout-btn')
        .addEventListener('click', logout);

    this.document.querySelector('#find-task-submit-btn')
        .addEventListener('click', findTaskById);

    this.document.querySelector('#find-tasks-by-status-submit-btn')
        .addEventListener('click', findTasksByStatus);
});

function logout() {
    postRequest(
        path + 'api/Auth/logout'
    )
        .then(response => {
            window.location.replace(path + 'index.html');
        })
        .catch(error => console.error(error))
}

function findTaskById(event) {
    console.log('findTaskById');
    var Id;
    console.log(event.currentTarget.taskId);
    if (event.currentTarget.taskId != undefined) {
        Id = event.currentTarget.taskId;
    }
    else {
        Id = document.querySelector('#task-id-input').value;
    }
    getRequest(
        path + 'api/Courses/task/' + Id.toString()
    )
        .then(response => showTask(response))
        .catch(error => { console.error(error), showAlert('Failed to find task.') });
}

function findTasksByStatus() {
    console.log('findTasksByStatus');
    var status = document.querySelector('#find-tasks-by-status-form')
        .querySelector('.form-select-sm').value;
    getRequest(
        path + 'api/Courses/getTasks?status=' + status.toString()
    )
        .then(response => showTasks(response))
        .catch(error => { console.error(error), showAlert('Failed to find tasks.') });
}

function showTask(json) {
    var tasks_place = document.querySelector('.tasks');
    tasks_place.innerHTML = '';
    var new_task = document.querySelector('.task-card').cloneNode(true);

    new_task.querySelector('.task-id').innerHTML += json['taskId']; 
    new_task.querySelector('.task-status').innerHTML += json['status']; 
    new_task.querySelector('.courses-created').innerHTML += json['coursesCreated']; 
    new_task.querySelector('.courses-assigned').innerHTML += json['coursesAssigned'];
    new_task.querySelector('.view-courses-btn').setAttribute('href',
        new_task.querySelector('.view-courses-btn').getAttribute('href') + json['taskId']);

    new_task.classList.remove('d-none');
    tasks_place.appendChild(new_task);
}

function showTasks(json) {
    var tasks_place = document.querySelector('.tasks');
    tasks_place.innerHTML = '';
    var originalTaskCard = document.querySelector('.task-short-card');

    for (let i = 0; i < json.length; i++) {
        var new_task = originalTaskCard.cloneNode(true);

        new_task.querySelector('.task-id').innerHTML += json[i]['taskId'];
        new_task.querySelector('.task-status').innerHTML += json[i]['status'];
        var detailedInfoBtn = new_task.querySelector('.view-detailed-info-btn')
        detailedInfoBtn.addEventListener('click', findTaskById);
        detailedInfoBtn.taskId = json[i]['taskId'];

        new_task.classList.remove('d-none');
        tasks_place.appendChild(new_task);
    }
}

function showAlert(message) {
    console.log('showAlert');
    var alertsBox = document.querySelector('.alerts-box');
    var newAlert = document.querySelector('.alert').cloneNode(true);
    newAlert.querySelector('.message').innerHTML = message;
    newAlert.classList.remove('d-none');
    alertsBox.appendChild(newAlert);
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

function getRequest(url) {
    return fetch(url,
        {
            method: "GET"
        }
    ).then(response => response.json());
}
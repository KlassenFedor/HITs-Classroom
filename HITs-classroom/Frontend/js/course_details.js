window.addEventListener('load', function () {
    const updateButton = this.document.querySelector('#invitations-update-btn')
        .addEventListener('click', getCourseInvitations);
    const createButton = this.document.querySelector('#createInvitationBtn')
        .addEventListener('click', createInvitation);

    getCourseInvitations();
});


//--------Invitations--------

function getCourseInvitations() {
    console.log('getCourseInvitations');
    postRequestWithoutResponseBody(
        'https://localhost:7284/api/Invitations/update/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => () => { })
        .catch(error => { console.error(error), alert('Invitations are currently unavailable') })

    getRequest(
        'https://localhost:7284/api/Invitations/list/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => (showInvitationsList(response)))
        .catch(error => { console.error(error), alert('Invitations are currently unavailable') })
}

function deleteInvitation(event) {
    console.log('deleteinvitation');
    deleteRequest(
        'https://localhost:7284/api/Invitations/delete/' + event.currentTarget.invitationId
    )
        .then(response => () => { document.querySelector("[id='" + event.currentTarget.invitationId + "']").remove() })
        .catch(error => { console.error(error), alert('Unable to delete invitation') })
}

function createInvitation() {
    console.log('createInvitation');
    var data = new Object();
    for (const pair of new FormData(document.querySelector('#createInvitationForm'))) {
        if (pair[1] != null && pair[1] != '') {
            data[pair[0]] = pair[1];
        }
    }
    data['courseId'] = window.location.href.split('?')[1].split('=')[1];
    data = JSON.stringify(data);
    postRequest(
        'https://localhost:7284/api/Invitations/create',
        data
    )
        .then(response => alert('Created successfully'))
        .catch(error => { console.error(error), alert('Failed to create invitation') })
}

function showInvitationsList(json) {
    const invitationsPlace = document.querySelector('#invitationsPlace');
    invitationsPlace.innerHTML = '';
    var invitationRow = document.querySelector('.invitationRow').cloneNode(true);
    for (let i = 0; i < json.length; i++) {
        invitationRow.querySelector('.invitation-number').innerHTML = i + 1;
        invitationRow.querySelector('.invitation-email').innerHTML = json[i]['email'];
        invitationRow.querySelector('.invitation-role').innerHTML = json[i]['role'];
        var status = 'accepted';
        invitationRow.querySelector('.invitation-status').classList.add('text-success');
        if (!json[i]['isAccepted']) {
            status = 'not accepted';
            invitationRow.querySelector('.invitation-status').classList.add('text-danger');
        }
        invitationRow.querySelector('.invitation-status').innerHTML = status;
        invitationRow.querySelector('.invitation-last-update').innerHTML = json[i]['updateTime'];
        var deleteButton = invitationRow.querySelector('.invitation-delete');
        deleteButton.addEventListener('click', deleteInvitation);
        deleteButton.invitationId = json[i]['id'];
        invitationRow.classList.remove('d-none');
        invitationRow.id = json[i]['id'];
        invitationsPlace.appendChild(invitationRow);
    }
}

//--------Members--------

function getMembers() {
    getTeachers();
    getStudents();
}

function getTeachers() {

}

function getStudents() {

}

function deleteStudent(event) {
    console.log('deleteStudent');
    deleteRequest(
        'https://localhost:7284/api/Coursemembers/delete/courses/'
            + window.location.href.split('?')[1].split('=')[1]
            + '/students' + event.currentTarget.studentId
    )
        .then(response => () => { document.querySelector("[id='" + event.currentTarget.studentId + "']").remove() })
        .catch(error => { console.error(error), alert('Unable to delete student') })
}

function deleteTeacher(event) {
    console.log('deleteTeacher');
    deleteRequest(
        'https://localhost:7284/api/Coursemembers/delete/courses/'
        + window.location.href.split('?')[1].split('=')[1]
        + '/teachers' + event.currentTarget.teacherId
    )
        .then(response => () => { document.querySelector("[id='" + event.currentTarget.teacherId + "']").remove() })
        .catch(error => { console.error(error), alert('Unable to delete teacher') })
}

function showTeachers(json) {
    const teachersPlace = document.querySelector('#teachersPlace');
    teachersPlace.innerHTML = '';
    var teachersPlace = document.querySelector('.teacherRow').cloneNode(true);
    for (let i = 0; i < json.length; i++) {
        teachersPlace.querySelector('.teacher-number').innerHTML = i + 1;
        teachersPlace.querySelector('.teacher-email').innerHTML = json[i]['email'];
        teachersPlace.querySelector('.teacher-name').innerHTML = json[i]['name'];
        var deleteButton = teachersPlace.querySelector('.teacher-delete');
        deleteButton.addEventListener('click', deleteTeacher);
        deleteButton.teacherId = json[i]['id'];
        teachersPlace.classList.remove('d-none');
        teachersPlace.id = json[i]['email'];
        teachersPlace.appendChild(teachersPlace);
    }
}

function showStudents(json) {
    const studentsPlace = document.querySelector('#studentsPlace');
    studentsPlace.innerHTML = '';
    var studentsPlace = document.querySelector('.studentRow').cloneNode(true);
    for (let i = 0; i < json.length; i++) {
        studentsPlace.querySelector('.student-number').innerHTML = i + 1;
        studentsPlace.querySelector('.student-email').innerHTML = json[i]['email'];
        studentsPlace.querySelector('.student-name').innerHTML = json[i]['name'];
        var deleteButton = studentsPlace.querySelector('.student-delete');
        deleteButton.addEventListener('click', deleteStudent);
        deleteButton.studentId = json[i]['id'];
        studentsPlace.classList.remove('d-none');
        studentsPlace.id = json[i]['email'];
        studentsPlace.appendChild(studentsPlace);
    }
}

//--------Wrappers for HTTP methods--------

function getRequest(url) {
    return fetch(url,
        {
            method: "GET"
        }
    ).then(response => response.json());
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
    ).then(response => response.json());
}

function deleteRequest(url) {
    return fetch(url,
        {
            method: "DELETE"
        }
    ).then(response => response.json());
}
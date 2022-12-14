const path = 'https://localhost:7284/';

window.addEventListener('load', function () {

    document.querySelector('#invitations-update-btn')
        .addEventListener('click', getCourseInvitations);

    document.querySelector('#createInvitationBtn')
        .addEventListener('click', createInvitation);

    document.querySelector('#members-get-btn')
        .addEventListener('click', getMembers);

    document.querySelector('#course-update-teachers-button')
        .addEventListener('click', updateTeachersInvitations);

    document.querySelector('#get-grades-btn')
        .addEventListener('click', getGrades);

    document.querySelector('#get-course-works-btn')
        .addEventListener('click', getCourseWorks);

    document.querySelector('#logout-btn')
        .addEventListener('click', logout);

    document.querySelector('#invitations-create-btn')
        .addEventListener('click', findTeachers);

    setCourseInfo();
    getMembers();
    getCourseInvitations();
    getCourseWorks();
});


//--------Invitations--------

function getCourseInvitations() {
    console.log('getCourseInvitations');
    postRequestWithoutResponseBody(
        path + 'api/Invitations/update/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => () => { })
        .catch(error => { console.error(error), showAlert('Invitations are currently unavailable') })

    getRequest(
        path + 'api/Invitations/list/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => (showInvitationsList(response)))
        .catch(error => { console.error(error), showAlert('Invitations are currently unavailable') })
}

function deleteInvitation(event) {
    console.log('deleteinvitation');
    deleteRequest(
        path + 'api/Invitations/delete/' + event.currentTarget.invitationId
    )
        .then(response => () => { document.querySelector("[id='" + event.currentTarget.invitationId + "']").remove() })
        .catch(error => { console.error(error), showAlert('Unable to delete invitation') })
}

function resendInvitation(event) {
    console.log('resendInvitation');
    postRequestWithoutResponseBody(
        path + 'api/Invitations/resend/' + event.currentTarget.invitationId
    )
        .then(response => () => { getCourseInvitations() })
        .catch(error => { console.error(error), showAlert('Unable to delete invitation') })
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
        path + 'api/Invitations/create',
        data
    )
        .then(response => {
            if (response.ok) {
                showAlert('Created successfully.');
            } else {
                showAlert('Failed to create invitation.');
            }
        })
        .catch(error => { console.error(error), showAlert('Failed to create invitation') })
}

function showInvitationsList(json) {
    const invitationsPlace = document.querySelector('#invitationsPlace');
    invitationsPlace.innerHTML = '';
    for (let i = 0; i < json.length; i++) {
        var invitationRow = document.querySelector('.invitationRow').cloneNode(true);
        invitationRow.querySelector('.invitation-number').innerHTML = i + 1;
        invitationRow.querySelector('.invitation-email').innerHTML = json[i]['email'];
        invitationRow.querySelector('.invitation-role').innerHTML = json[i]['role'];
        var status = 'accepted';
        invitationRow.querySelector('.invitation-status').classList.add('text-success');
        invitationRow.querySelector('.invitation-status').classList.remove('text-danger');
        if (!json[i]['isAccepted']) {
            status = 'not accepted';
            invitationRow.querySelector('.invitation-status').classList.remove('text-success');
            invitationRow.querySelector('.invitation-status').classList.add('text-danger');
        }
        invitationRow.querySelector('.invitation-status').innerHTML = status;
        invitationRow.querySelector('.invitation-last-update').innerHTML = json[i]['updateTime'];
        var deleteButton = invitationRow.querySelector('.invitation-delete');
        deleteButton.addEventListener('click', deleteInvitation);
        deleteButton.invitationId = json[i]['id'];
        var resendButton = invitationRow.querySelector('.invitation-resend');
        resendButton.addEventListener('click', resendInvitation);
        resendButton.invitationId = json[i]['id'];
        invitationRow.classList.remove('d-none');
        invitationRow.id = json[i]['id'];
        invitationsPlace.appendChild(invitationRow);
    }
}

function updateTeachersInvitations() {
    console.log('updateTeachersInvitations');
    getRequest(
        path + 'api/Invitations/checkTeachersInvitations/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => showCourseStatus(response))
        .catch(error => { console.error(error), showAlert('Unable to check teachers invitations') })
}

function showCourseStatus(response) {
    const statusPlace = document.querySelector('#course-has-all-teachers');
    statusPlace.classList.remove('text-danger');
    statusPlace.classList.remove('text-success');
    if (response) {
        statusPlace.innerHTML = 'All teachers accepted invitations';
        statusPlace.classList.add('text-success');
    }
    else {
        statusPlace.innerHTML = 'Not all teachers accepted invitations';
        statusPlace.classList.add('text-danger');
    }
}

function findTeachers() {
    console.log('findTeachers');
    getRequest(
        path + 'api/TeachersSearch/findTeachers/' + ''
    )
        .then(response => addTeachersToList(response))
        .catch(error => { console.error(error)})
}

function addTeachersToList(teachers) {
    var suitableTeachers = document.querySelector('#teachers');
    suitableTeachers.innerHTML = '';
    var teachersList = "";
    for (let i = 0; i < teachers.length; i++) {
        teachersList = teachersList +
            '<option value="' + teachers[i]['email'] + '">'
            + teachers[i]['name'] + '</option>';
    }
    suitableTeachers.innerHTML = teachersList;
}

//--------Members--------

function getMembers() {
    getTeachers();
    getStudents();
}

function getTeachers() {
    console.log('getTeachers');
    getRequest(
        path + 'api/CourseMembers/teachers/list/'
        + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => showTeachers(response))
        .catch(error => { console.error(error), showAlert('Unable to get teachers') })
}

function getStudents() {
    console.log('getStudents');
    getRequest(
        path + 'api/CourseMembers/students/list/'
        + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => showStudents(response))
        .catch(error => { console.error(error), showAlert('Unable to get students') })
}

function deleteStudent(event) {
    console.log('deleteStudent');
    deleteRequest(
        path + 'api/CourseMembers/delete/courses/'
            + window.location.href.split('?')[1].split('=')[1]
            + '/students/' + event.currentTarget.studentId
    )
        .then(response => () => { document.querySelector("[id='" + event.currentTarget.studentId + "']").remove() })
        .catch(error => { console.error(error), showAlert('Unable to delete student') })
}

function deleteTeacher(event) {
    console.log('deleteTeacher');
    deleteRequest(
        path + 'api/CourseMembers/delete/courses/'
        + window.location.href.split('?')[1].split('=')[1]
        + '/teachers/' + event.currentTarget.teacherId
    )
        .then(response => () => { document.querySelector("[id='" + event.currentTarget.teacherId + "']").remove() })
        .catch(error => { console.error(error), showAlert('Unable to delete teacher') })
}

function showTeachers(json) {
    const teachersPlace = document.querySelector('#teachersPlace');
    teachersPlace.innerHTML = '';
    for (let i = 0; i < json.length; i++) {
        var teacherRow = document.querySelector('.teacherRow').cloneNode(true);
        teacherRow.querySelector('.teacher-number').innerHTML = i + 1;
        teacherRow.querySelector('.teacher-email').innerHTML = json[i]['email'];
        teacherRow.querySelector('.teacher-name').innerHTML = json[i]['name'];
        teacherRow.classList.remove('d-none');
        var deleteButton = teacherRow.querySelector('.teacher-delete');
        deleteButton.addEventListener('click', deleteTeacher);
        deleteButton.teacherId = json[i]['email'];
        teacherRow.id = json[i]['email'];
        teachersPlace.appendChild(teacherRow);
    }
}

function showStudents(json) {
    const studentsPlace = document.querySelector('#studentsPlace');
    studentsPlace.innerHTML = '';
    for (let i = 0; i < json.length; i++) {
        var studentRow = document.querySelector('.studentRow').cloneNode(true);
        studentRow.querySelector('.student-number').innerHTML = i + 1;
        studentRow.querySelector('.student-email').innerHTML = json[i]['email'];
        studentRow.querySelector('.student-name').innerHTML = json[i]['name'];
        var deleteButton = studentRow.querySelector('.student-delete');
        deleteButton.addEventListener('click', deleteStudent);
        deleteButton.studentId = json[i]['email'];
        studentRow.classList.remove('d-none');
        studentRow.id = json[i]['email'];
        studentsPlace.appendChild(studentRow);
    }
}

//--------Course works and grades----------

function getGrades() {
    console.log('getGrades');
    getRequest(
        path + 'api/CourseWorks/courseGrades/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => console.log(response))
        .catch(error => { console.error(error), showAlert('Unable to get grades') })
}

function getCourseWorks() {
    console.log('getCourseWorks');
    getRequest(
        path + 'api/CourseWorks/courseWorks/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => showCourseWorks(response))
        .catch(error => { console.error(error), showAlert('Unable to get course works') })
}

function showCourseWorks(works) {
    var worksPlace = document.querySelector('#course-works-place');
    worksPlace.innerHTML = '';
    for (let i = 0; i < works.length; i++) {
        var courseWorkClone = document.querySelector('.course-work').cloneNode(true);
        courseWorkClone.querySelector('.course-work-title').innerHTML = works[i]['title'];
        courseWorkClone.querySelector('.works-passed').innerHTML = 'Works passed: ' + works[i]['worksPassed'];
        courseWorkClone.querySelector('.works-evaluted').innerHTML = 'Works evaluted: ' + works[i]['worksEvaluted'];
        courseWorkClone.classList.remove('d-none');
        worksPlace.appendChild(courseWorkClone);
    }
}

//--------Course info--------

function setCourseInfo() {
    console.log('setCourseInfo');
    getRequest(
        path + 'api/Courses/get/' + 
        + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => showCourseInfo(response))
        .catch(error => { console.error(error), showAlert('Unable to get course info') })
}

function showCourseInfo(json) {
    var courseName = document.querySelector('#course-info-name');
    var courseId = document.querySelector('#course-info-id');
    var courseState = document.querySelector('#course-info-state');
    var courseTeachers = document.querySelector('#course-has-all-teachers');
    courseName.innerHTML = json['name'];
    courseId.innerHTML = json['courseId'];
    courseState.innerHTML = json['courseState'];
    showCourseStatus(json['hasAllTeachers']);
}

//--------Auth--------

function logout() {
    postRequest(
        path + 'api/Auth/logout'
    )
        .then(response => {
            window.location.replace(path + 'index.html');
        })
        .catch(error => console.error(error))
}

//--------Alerts--------

function showAlert(message) {
    console.log('showAlert');
    var alertsBox = document.querySelector('.alerts-box');
    var newAlert = document.querySelector('.alert').cloneNode(true);
    newAlert.querySelector('.message').innerHTML = message;
    newAlert.classList.remove('d-none');
    alertsBox.appendChild(newAlert);
}

//--------Implementing for HTTP methods--------

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
    ).then(response => response);
}

function deleteRequest(url) {
    return fetch(url,
        {
            method: "DELETE"
        }
    ).then(response => response);
}
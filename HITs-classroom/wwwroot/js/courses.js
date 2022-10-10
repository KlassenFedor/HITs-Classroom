const path = 'https://localhost:7284/';

window.addEventListener('load', function () {

    document.querySelector('#searchCourseSubmitButton')
        .addEventListener("click", findCourses);

    document.querySelector('#searchCourseByIdSubmitButton')
        .addEventListener("click", findCourseById);

    document.querySelector('#createCourseSubmitButton')
        .addEventListener("click", createCourse);

    document.querySelector('#sync-courses-button')
        .addEventListener('click', syncCourses);

    document.querySelector('#logout-btn')
        .addEventListener('click', logout);

    findActiveCourses();
});

//--------Adding, deleting, changing courses on the page--------

function convertCoursesFromJsonToArray(json) {
    console.log('convertCoursesFromJsonToArray');
    let newArray = [];
    for (let i = 0; i < json.length; i++) {
        newArray.push(prepareCourseFromJson(json[i]));
    }

    return newArray;
}

//creating a course card from json
function prepareCourseFromJson(course) {
    console.log('prepareCourseFromJson');
    let courseClone = document.querySelector('#course-card-example').cloneNode(true);
    courseClone.classList.remove('d-none');
    courseClone.setAttribute('id', course['id']);
    courseClone = prepareCourseCard(course, courseClone);

    return courseClone;
}

//filling course card fields from json
function prepareCourseCard(course, courseClone) {

    //course fields filling
    courseClone.id = course['courseId'];
    let courseCardLink = courseClone.querySelector('.course-card-link');
    courseCardLink.setAttribute('href', courseCardLink.getAttribute('href').split('=')[0] + '=' + course['courseId']);
    let courseName = courseClone.querySelector('.course-name');
    courseName.innerText = course['name'];
    let courseSection = courseClone.querySelector('.course-section');
    courseSection.innerText = course['section'];
    let courseDescriptionHeading = courseClone.querySelector('.course-description-heading');
    courseDescriptionHeading.innerText = course['descriptionHeading'];
    let courseEnrollmentCode = courseClone.querySelector('.course-enrollment-code');
    courseEnrollmentCode.innerText = course['enrollmentCode'];
    let courseState = courseClone.querySelector('.course-state');
    courseState.innerText = course['courseState'];
    let hasAllTeachers = courseClone.querySelector('.has-all-course-teachers');
    if (course['hasAllTeachers']) {
        hasAllTeachers.classList.add('text-success');
        hasAllTeachers.innerText = 'Has all teachers'
    }
    else {
        hasAllTeachers.classList.add('text-danger');
        hasAllTeachers.innerText = 'Has not all teachers'
    }

    //adding event listeners
    let delButton = courseClone.querySelector('.del-button');
    if (course['courseState'] != "ARCHIVED") {
        delButton.classList.add('disabled');
    }
    else {
        delButton.classList.remove('disabled');
    }
    delButton.addEventListener('click', deleteCourse);
    delButton.currentId = course['courseId'];

    let archiveButton = courseClone.querySelector('.archive-button');
    archiveButton.addEventListener('click', archiveCourse);
    archiveButton.currentId = course['courseId'];

    let editButton = courseClone.querySelector('.edit-button');
    editButton.addEventListener('click', fillModalForEditing);
    editButton.currentId = course['courseId'];
    editButton.currentName = course['name'];
    editButton.currentOwnerId = course['ownerId'];
    editButton.currentDescription = course['description'];
    editButton.currentDescriptionHeading = course['descriptionHeading'];
    editButton.currentRoom = course['room'];
    editButton.currentSection = course['section'];
    editButton.currentCourseState = course['courseState'];

    let saveChangesBtn = document.querySelector('#editCourseBtn');
    saveChangesBtn.addEventListener('click', editCourse);

    return courseClone;
}

function addCoursesToPage(courses) {
    console.log('addCoursesToPage');
    let placeForCourses = document.querySelector('#allCourses');
    placeForCourses.innerHTML = '';
    for (let i = 0; i < courses.length; i++) {
        placeForCourses.appendChild(courses[i]);
    }
}

function editCourseCard(course) {
    console.log(course);
    let courseCard = document.querySelector("[id='" + course['courseId'] + "']");
    courseCard = prepareCourseCard(course, courseCard);
}

function deleteCourseCard(Id) {
    let courseCard = document.querySelector("[id='" + Id + "']");
    let placeForCourses = document.querySelector('#allCourses');
    placeForCourses.removeChild(courseCard);
}

function fillModalForEditing(event) {
    let editingModal = document.querySelector('#editCourseModal');
    let editCourseForm = editingModal.querySelector('#editCourseForm');
    editingModal.querySelector('#courseIdForModalHeader').innerHTML = event.currentTarget.currentId;
    editCourseForm.querySelector('#courseName_Editing').value = event.currentTarget.currentName;
    editCourseForm.querySelector('#courseRoom_Editing').value = event.currentTarget.currentRoom;
    editCourseForm.querySelector('#courseSection_Editing').value = event.currentTarget.currentSection;
    editCourseForm.querySelector('#courseDescription_Editing').value = event.currentTarget.currentDescription;
    editCourseForm.querySelector('#courseDescriptionHeading_Editing').value = event.currentTarget.currentDescriptionHeading;
    editCourseForm.querySelector('#courseState_Editing').value = event.currentTarget.currentCourseState;
}

//--------Wrappers for HTTP methods--------

function logout() {
    postRequest(
        path + 'api/Auth/logout'
    )
        .then(response => {
            window.location.replace(path + 'index.html');
        })
        .catch(error => console.error(error))
}

function archiveCourse(event) {
    console.log('archiveCourse');
    var data = new Object;
    data = JSON.stringify(data);
    patchRequest(
        path + 'api/Courses/archive/' + event.currentTarget.currentId.toString(),
        data
    )
        .then(response => editCourseCard(response))
        .catch(error => () => { console.error(error), alert('Failed to archive course') })
}

function editCourse() {
    console.log('editCourse');
    var data = new Object();
    for (const pair of new FormData(document.querySelector('#editCourseForm'))) {
        if (pair[1] != null && pair[1] != '') {
            data[pair[0]] = pair[1];
        }
    }
    data['ownerId'] = 'me';
    if (data['name'] == null || data['ownerId'] == null) {
        alert('Fields "name" and "ownerId" must be filled');
    }
    else {
        data = JSON.stringify(data);
        putRequest(
            path + 'api/Courses/update/' + document.querySelector('#courseIdForModalHeader').innerHTML,
            data
        )
            .then(response => editCourseCard(response))
            .catch(error => { console.error(error), alert('Failed to edit course') })
    }
}

function deleteCourse(event) {
    console.log('deleteCourse');
    deleteRequest(
        path + 'api/Courses/delete/' + event.currentTarget.currentId.toString()
    )
        .then(deleteCourseCard(event.currentTarget.currentId.toString()))
        .catch(error => { console.error(error), alert('Failed to delete course') })
}

function findActiveCourses() {
    console.log('findActiveCourses');
    getRequest(
        path + 'api/Courses/active'
    )
        .then(response => addCoursesToPage(convertCoursesFromJsonToArray(response)))
        .catch(error => {console.error(error), alert('Courses are currently unavailable') })
}

function findCourses() {
    console.log('findCourses');
    let queryParameters = '';
    for (const pair of new FormData(document.querySelector('#searchCoursesForm'))) {
        if (pair[1] != null && pair[1] != '') {
            queryParameters = queryParameters + '&' + pair[0].toString() + '=' + pair[1].toString();
        }
    }
    queryParameters = queryParameters.slice(1);
    console.log(queryParameters);
    getRequest(
        path + 'api/Courses/listFromDb?' + queryParameters
    )
        .then(response => addCoursesToPage(convertCoursesFromJsonToArray(response)))
        .catch(error => { console.error(error), alert('Failed to find courses') })
}

function findCourseById() {
    console.log('findCourseById');
    var Id = document.querySelector('#courseId_Serach').value;
    getRequest(
        path + 'api/Courses/get/' + Id.toString()
    )
        .then(response => addCoursesToPage([prepareCourseFromJson(response)]))
        .catch(error => { console.error(error), alert('Failed to find course.') });
}

function updateCoursesStatuses() {
    console.log('updateCoursesStatuses');
    postRequestWithoutResponseBody(
        path + 'api/Invitations/updateAll'
    )
        .catch(error => { console.error(error), alert('Failed to update courses.') });
}

function syncCourses() {
    postRequest(
        path + 'api/Courses/synchronize'
    )
        .then(response => {
            if (response.ok) {
                alert('Successfully synchronized');
            } else {
                alert('Failed to synchronize.');
            }
        })
        .catch(error => console.error(error));
}

function createCourse() {
    var data = new Object();
    for (const pair of new FormData(document.querySelector('#createCourseForm'))) {
        if (pair[1] != null && pair[1] != '') {
            data[pair[0]] = pair[1];
        }
    }
    data = JSON.stringify(data);
    postRequest(
        path + 'api/Courses/create',
        data
    )
        .then(response => {
            console.log(response.ok);
            if (response.ok) {
                alert('Created successfully.');
                response.json().then(res => {
                    window.location.href = path + 'pages/course_details.html?id=' + res['courseId']
                });
            } else {
                alert('Failed to create.');
            }
        })
        .catch(error => console.error(error));
}

//--------Implementing HTTP methods--------

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

function patchRequest(url, data) {
    return fetch(url,
        {
            method: "PATCH",
            body: data
        }
    ).then(response => response.json());
}

function putRequest(url, data) {
    return fetch(url,
        {
            method: "PUT",
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
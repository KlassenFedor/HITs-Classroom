﻿window.addEventListener('load', function () {

    const searchBtn = document.querySelector('#searchCourseSubmitButton');
    if (searchBtn) {
        searchBtn.addEventListener("click", findCourses);
    }

    const searchByIdBtn = document.querySelector('#searchCourseByIdSubmitButton');
    if (searchByIdBtn) {
        searchByIdBtn.addEventListener("click", findCourseById);
    }

    findActiveCourses();
});

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
    let courseClone = document.querySelector('.course-card').cloneNode(true);
    courseClone.classList.remove('d-none');
    courseClone.setAttribute('id', course['id']);
    courseClone = prepareCourseCard(course, courseClone);

    return courseClone;
}

//filling course card fields from json
function prepareCourseCard(course, courseClone) {
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

    let delButton = courseClone.querySelector('.del-button');
    if (course['courseState'] != "ARCHIVED") {
        delButton.classList.add('disabled');
    }
    else {
        delButton.classList.remove('disabled');
    }
    delButton.addEventListener('click', deleteCourse);
    delButton.currentId = course['id'];
    let archiveButton = courseClone.querySelector('.archive-button');
    archiveButton.addEventListener('click', archiveCourse);
    archiveButton.currentId = course['id'];
    let editButton = courseClone.querySelector('.edit-button');
    editButton.addEventListener('click', editCourse.bind(course['id']));

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
    let courseCard = document.querySelector("[id='" + course['id'] + "']");
    courseCard = prepareCourseCard(course, courseCard);
}

function deleteCourseCard(Id) {
    let courseCard = document.querySelector("[id='" + Id + "']");
    let placeForCourses = document.querySelector('#allCourses');
    placeForCourses.removeChild(courseCard);
}

function archiveCourse(event) {
    console.log('archiveCourse');
    var data = new Object;
    data = JSON.stringify(data);
    patchRequest(
        'https://localhost:7284/api/Courses/archive/' + event.currentTarget.currentId.toString(),
        data
    )
        .then(response => editCourseCard(response))
        .catch(error => console.error(error))
}

function editCourse() {

}

function deleteCourse(event) {
    console.log('deleteCourse');
    deleteRequest(
        'https://localhost:7284/api/Courses/delete/' + event.currentTarget.currentId.toString()
    )
        .then(deleteCourseCard(event.currentTarget.currentId.toString()))
        .catch(error => console.error(error))
}

function findActiveCourses() {
    console.log('findActiveCourses');
    var data = new Object();
    data.courseState = "ACTIVE";
    data = JSON.stringify(data);
    postRequest(
        'https://localhost:7284/api/Courses/Courses',
        data
    )
        .then(response => addCoursesToPage(convertCoursesFromJsonToArray(response)))
        .catch(error => console.error(error))
}

function findCourses() {
    console.log('findCourses');
    var data = new Object();
    for (const pair of new FormData(document.querySelector('#searchCoursesForm'))) {
        if (pair[1] != null && pair[1] != '') {
            data[pair[0]] = pair[1];
        }
    }
    data = JSON.stringify(data);
    console.log(data);
    postRequest(
        'https://localhost:7284/api/Courses/Courses',
        data
    )
        .then(response => addCoursesToPage(convertCoursesFromJsonToArray(response)))
        .catch(error => console.error(error))
}

function findCourseById() {
    console.log('findCourseById');
    var Id = document.querySelector('#courseId_Serach').value;
    getRequest(
        'https://localhost:7284/api/Courses/Get/' + Id.toString()
    )
        .then(response => addCoursesToPage([prepareCourseFromJson(response)]))
        .catch(error => console.error(error));
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

function deleteRequest(url) {
    return fetch(url,
        {
            method: "DELETE"
        }
    ).then(response => response.json());
}
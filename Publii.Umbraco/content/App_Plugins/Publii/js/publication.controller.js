angular.module("umbraco").controller("CreatePublicationController", function(
    $scope,
    $http,
    $window,
    $location,
    $routeParams,
    eventsService,
    editorService,
    notificationsService) {

    let vm = this;

    vm.urlSegmentLostFocus = urlSegmentLostFocus;
    vm.buttonState = "init";

    vm.formData = {
        name: '',
        urlSegment: '',
        description: '',
        fileData: null,
        selectedFileName: ''
    };

    vm.editData = {
        id: -1,
        name: '',
        urlSegment: '',
        description: ''
    };

    vm.editMode = "create"; // possible modes: "create", "edit"
    let urlParams = $location.search();
    if ('create' in urlParams) {
        vm.editMode = "create";
    } else {
        vm.editMode = "edit";
        setEditData();
    }

    function setEditData() {
        vm.editData.id = $routeParams.id;
        // Fetch publication data
        fetch('/umbraco/backoffice/publii/publications/get?id=' + vm.editData.id)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch publication data');
                }
                return response.text(); // Read response as text
            })
            .then(text => {
                // Remove the prefix if it exists
                const jsonText = text.replace(/^\)\]\}',/, '');
                // Parse the JSON
                return JSON.parse(jsonText);
            })
            .then(data => {
                // Handle the data here
                vm.editData.name = data.Name;
                vm.editData.urlSegment = data.UrlSegment;
                vm.editData.description = data.Description;
                $scope.$apply();
            })
            .catch(error => {
                console.error('Error fetching publication data:', error);
            });
    }

    function urlSegmentLostFocus() {
        if (vm.editMode === 'create') {
            vm.formData.urlSegment = makeValidUrlSegment(vm.formData.urlSegment);
        } else {
            vm.editData.urlSegment = makeValidUrlSegment(vm.editData.urlSegment);
        }
    }


    vm.triggerFileInput = function() {
        document.getElementById('fileInput').click();
    }

    vm.clickButton = function() {
        if (vm.editMode === 'create') {
            vm.submitForm();
        } else {
            vm.updatePublication();
        }
    }

    eventsService.on("data.error", function(event, arguments) {
        notificationsService.error('Error', arguments.message);
    });

    eventsService.on("data.success", function(event, arguments) {
        notificationsService.success('Success', arguments.message);
    });

    window.handleFileChange = function (event) {
        let files = event.target.files;
        if (files && files.length > 0) {
            vm.formData.fileData = files[0];
            vm.formData.selectedFileName = files[0].name;
            vm.formData.name = removeFileExtension(files[0].name);
            vm.formData.urlSegment = makeValidUrlSegment(removeFileExtension(files[0].name));
            // triggers the angular digest cycle for the ng-bind
            // since we are using a native onchange
            $scope.$apply();
        }
    };

    vm.updatePublication = function() {
        // Convert editData to JSON string
        const requestBody = JSON.stringify(vm.editData);

        fetch('/umbraco/backoffice/publii/publications/edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: requestBody
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to update publication');
                }
                return response.text(); // Read response as text
            })
            .then(text => {
                // Remove the prefix if it exists
                const jsonText = text.replace(/^\)\]\}',/, '');
                // Parse the JSON
                return JSON.parse(jsonText);
            })
            .then(data => {
                // Handle the response data here
                console.log('adsf');
                vm.buttonState = "success";
                eventsService.emit("data.success", { value: null, message: data.message });
            })
            .catch(error => {
                vm.buttonState = "error";
                eventsService.emit("data.error", { value: null, message: "An error occurred saving data, please consult logs." });
            });
    }

    vm.submitForm = function () {
        vm.buttonState = "busy";

        let formData = new FormData();
        formData.append('name', vm.formData.name);
        formData.append('urlSegment', vm.formData.urlSegment);
        formData.append('description', vm.formData.description);

        if (vm.formData.fileData) {
            formData.append('fileData', vm.formData.fileData);
            formData.append('selectedFileName', vm.formData.selectedFileName);
        }

        fetch('/umbraco/backoffice/publii/publications/uploadpublication', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                return response.text();
            })
            .then(dataString => {
                let data = trimDataString(dataString);
                handleSetDataSuccess(data);
            })
            .catch(error => handleSetDataError(error));
    };

    function handleSetDataSuccess(response) {
        vm.buttonState = "success";
        let data = JSON.parse(response);
        eventsService.emit("data.success", { value: null, message: data.message });
    }

    function handleSetDataError(response) {
        vm.buttonState = "error";
        eventsService.emit("data.error", { value: null, message: "An error occurred saving data, please consult logs." });
    }

    function trimDataString(dataString) {
        if (dataString.startsWith(')]}\',')) {
            return dataString.substring(5).trim();
        } else {
            return dataString.trim();
        }
    }

    function makeValidUrlSegment(inputString) {
        if (!inputString || typeof inputString !== 'string') {
            return '';
        }
        let lowercasedString = inputString.toLowerCase();
        let validUrlSegment = lowercasedString.replace(/\s+/g, '-');
        validUrlSegment = validUrlSegment.replace(/[^a-z0-9-]/g, '');
        validUrlSegment = validUrlSegment.replace(/^-+|-+$/g, '');
        return validUrlSegment;
    }

    function removeFileExtension(filename) {
        const lastDotIndex = filename.lastIndexOf('.');
        if (lastDotIndex !== -1 && lastDotIndex > 0) {
            return filename.substring(0, lastDotIndex);
        }
        return filename;
    }

});
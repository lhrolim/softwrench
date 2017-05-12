describe("Test For FirstSolar WorkPackage Screen", function () {



    var fsworkPackageService;

    beforeEach(module("sw_layout"));
    beforeEach(module("firstsolar"));

    var sharedItem = {
        "location": 10,
        "description": "loc",
        "summary": "s"
    }


    beforeEach(inject(function ($injector, _fsworkpackageService_, _$rootScope_, _restService_, _$httpBackend_, _contextService_, _redirectService_,_crudContextHolderService_) {
        fsworkPackageService = _fsworkpackageService_;
    }));

    it("Test Section redimensioning 1 intermediate full capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 1, { attribute: 'E' });


        expect(redimensionedSection.displayables.length).toBe(2);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('E');
        expect(firstSection.displayables[2].attribute).toBe('B');
        expect(firstSection.displayables[3].attribute).toBe('C');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(1);
        expect(secondSection.displayables[0].attribute).toBe('D');

    });

    it("Test Section redimensioning adding at tail 1 intermediate full capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 5, { attribute: 'E' });


        expect(redimensionedSection.displayables.length).toBe(2);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('C');
        expect(firstSection.displayables[3].attribute).toBe('D');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(1);
        expect(secondSection.displayables[0].attribute).toBe('E');

    });

    it("Test Section redimensioning 2 intermediate full capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }]),
                SectionPojo.WithDisplayables([{ attribute: 'E' }, { attribute: 'F' }, { attribute: 'G' }, { attribute: 'H' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 1, { attribute: 'AA' });


        expect(redimensionedSection.displayables.length).toBe(3);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('AA');
        expect(firstSection.displayables[2].attribute).toBe('B');
        expect(firstSection.displayables[3].attribute).toBe('C');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(4);
        expect(secondSection.displayables[0].attribute).toBe('D');
        expect(secondSection.displayables[1].attribute).toBe('E');
        expect(secondSection.displayables[2].attribute).toBe('F');
        expect(secondSection.displayables[3].attribute).toBe('G');

        let thirdSection = redimensionedSection.displayables[2];
        expect(thirdSection.displayables.length).toBe(1);
        expect(thirdSection.displayables[0].attribute).toBe('H');

    });

    it("Test Section redimensioning 2 intermediate full capacity test 2", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }]),
                SectionPojo.WithDisplayables([{ attribute: 'E' }, { attribute: 'F' }, { attribute: 'G' }, { attribute: 'H' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 5, { attribute: 'EE' });


        expect(redimensionedSection.displayables.length).toBe(3);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('C');
        expect(firstSection.displayables[3].attribute).toBe('D');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(4);
        expect(secondSection.displayables[0].attribute).toBe('E');
        expect(secondSection.displayables[1].attribute).toBe('EE');
        expect(secondSection.displayables[2].attribute).toBe('F');
        expect(secondSection.displayables[3].attribute).toBe('G');

        let thirdSection = redimensionedSection.displayables[2];
        expect(thirdSection.displayables.length).toBe(1);
        expect(thirdSection.displayables[0].attribute).toBe('H');

    });

    it("Test Section redimensioning 1 intermediate half capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }]),
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 3, { attribute: 'C' });


        expect(redimensionedSection.displayables.length).toBe(1);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(3);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('C');

    });

    it("Test Section redimensioning 2 intermediate half capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }]),
                SectionPojo.WithDisplayables([{ attribute: 'E' }, { attribute: 'F' }, { attribute: 'G' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 5, { attribute: 'EE' });


        expect(redimensionedSection.displayables.length).toBe(2);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('C');
        expect(firstSection.displayables[3].attribute).toBe('D');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(4);
        expect(secondSection.displayables[0].attribute).toBe('E');
        expect(secondSection.displayables[1].attribute).toBe('EE');
        expect(secondSection.displayables[2].attribute).toBe('F');
        expect(secondSection.displayables[3].attribute).toBe('G');
        

    });

    it("Test Section redimensioning 2 removal intermediate half capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }]),
                SectionPojo.WithDisplayables([{ attribute: 'E' }, { attribute: 'F' }, { attribute: 'G' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 5);


        expect(redimensionedSection.displayables.length).toBe(2);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('C');
        expect(firstSection.displayables[3].attribute).toBe('D');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(2);
        expect(secondSection.displayables[0].attribute).toBe('E');
        expect(secondSection.displayables[1].attribute).toBe('G');


    });

    it("Test Section redimensioning 2 removal intermediate half capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }]),
                SectionPojo.WithDisplayables([{ attribute: 'E' }, { attribute: 'F' }, { attribute: 'G' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 2);


        expect(redimensionedSection.displayables.length).toBe(2);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('D');
        expect(firstSection.displayables[3].attribute).toBe('E');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(2);
        expect(secondSection.displayables[0].attribute).toBe('F');
        expect(secondSection.displayables[1].attribute).toBe('G');


    });

    it("Test Section redimensioning 3 removal intermediate half capacity", function () {

        const outerSection = {
            displayables: [
                SectionPojo.WithDisplayables([{ attribute: 'A' }, { attribute: 'B' }, { attribute: 'C' }, { attribute: 'D' }]),
                SectionPojo.WithDisplayables([{ attribute: 'E' }, { attribute: 'F' }, { attribute: 'G' }, { attribute: 'H' }]),
                SectionPojo.WithDisplayables([{ attribute: 'I' }])
            ]
        }

        //real call
        //(componentSection, indexToConsider, itemToInsert)
        const redimensionedSection = fsworkPackageService.redimensionIntermediateSections(outerSection, 2);


        expect(redimensionedSection.displayables.length).toBe(2);
        let firstSection = redimensionedSection.displayables[0];
        expect(firstSection.displayables.length).toBe(4);

        expect(firstSection.displayables[0].attribute).toBe('A');
        expect(firstSection.displayables[1].attribute).toBe('B');
        expect(firstSection.displayables[2].attribute).toBe('D');
        expect(firstSection.displayables[3].attribute).toBe('E');

        let secondSection = redimensionedSection.displayables[1];
        expect(secondSection.displayables.length).toBe(4);
        expect(secondSection.displayables[0].attribute).toBe('F');
        expect(secondSection.displayables[1].attribute).toBe('G');
        expect(secondSection.displayables[2].attribute).toBe('H');
        expect(secondSection.displayables[3].attribute).toBe('I');


    });

   


});
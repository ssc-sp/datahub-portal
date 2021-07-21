# Introduction

## Scope

This Concept of Operations (ConOps) provides a system description for NRCan DataHub. It is written for NRCan practitioners involved in the planning, development, design, operation, and maintenance of the DataHub. The ConOps document is intended to be part of an effort to
 collect requirements, develop DataHub concepts and configurations, and to establish how the DataHub components shall operate and interact in the future. The Draft and Final versions of the ConOps is intended to be a "living" document that reflects the evolving requirements for the DataHub. This ConOps document along with the Requirements Document forms the basis for the development of the various components within the DataHub. The document will provide the reader the following information:

-   Identification of the issues and problems to be resolved;

-   The agency needs to operate and manage the DataHub;

-   Operational and use perspectives to define the use and intent of the
    DataHub.

## Background

At NRCAN, data has largely been siloed and managed on a
 project-by-project basis without consideration for data as a strategic Enterprise asset. There is a significant need for a unified approach to managing data which ensures that the quality of data assets is maintained and the value can be extracted to the benefit of the organization.

NRCAN practitioners require the ability to discover data, access it efficiently, and collaborate effectively with others in order to achieve high levels of innovation. Without a unified cataloguing and management solution, practitioners are challenged to effectively share data and use it in innovative ways. The organization needs to simplify data access and reduce barriers to entry if it is to realize its innovation potential.

Numerous products and platforms have been developed with overlapping functionality and limited reuse. Sharing resources to develop common services (e.g., cataloguing, storage, data discovery) will not only reduce operational costs but ensure continual development and maintenance of strategic capabilities on behalf of the entire organization.

The NRCAN DataHub is a complex and ambitious undertaking because it deals with complex security models, state-of-the-art BI and analytics tools, and sophisticated data science environments that are accessible through a custom unified portal.

As the NRCAN landscape changes and adapts to internal and external influences/pressures, the needs of the NRCAN DataHub users and priority of features are expected to change. 

Over time, this will influence the costing and architecture. Our intent is to stay closely aligned with ePMB and ARB with regular updates throughout the year to
 ensure these changes are well understood and managed.

To be able to rapidly pivot based on changes on business need, we will employ Agile methodologies to deliver large groups of functionalities;
- the first being the Foundation of DataHub as described previously

- DataHub will follow NRCAN policy and will only be the system of record for select curated datasets:

- Migrated Data

    - Data migrated to DataHub will be managed throughout its lifecycle (e.g., collection, preservation, use, sharing, deletion). Lifecycle management will be in line with the data retention policies of the organization and source.

    - Migrated databases may be optimized (e.g., redesigned, compressed, enhanced) depending on the business need. Migration of undocumented, low quality, poorly designed or proprietary databases will be up to the discretion of DataHub. Migrated databases will undergo an architectural analysis and potential redesign before being migrated.

    - Migrated files will be available in the original format but may be managed (e.g., encrypted, moved, compressed, archived, replicated) as required. Files valuable to the organization will eventually be propagated to NRCAN Document Management solutions (e.g., GC Docs, SharePoint). DataHub will not be the system of
        record for these files.

- Integration with Other Data Solutions & Repositories

    - NRCAN DataHub as a central repository for managing most data (excluding files and documents) whether on same CSP or not.
    - DataHub will firm up the integration patterns with other DataHub-like solutions as part of the Foundation.

## Stakeholders

### Business User

The Business User is responsible for delivering value to the organization using IM/IT services. Responsibilities may be focused on creating strategy, developing plans or contributing to programs, portfolios and/or projects. They use business intelligence tools and data for evidence-based decisions, performance monitoring and planning.

#### Requirements
-   Business intelligence tools for reports and dashboards.

-   Data dictionaries and other documentation to make use of data.

-   Ability to store, retrieve, share and use information (e.g., data,
    documents, images).

-   Data lineage and traceability.

-   Integrated collaboration tools.

-   Ability to integrate new technologies into existing business
    processes.

### Data Scientist

The Data Scientist is responsible for conducting advanced analytics within the organization. Focused on research and development (R&D) and its applications, they experiment with new and emerging technologies to address complex business problems and develop insights for Business
 Users. Often their approach must be flexible and focused on a specific outcome for a client.

#### Requirements

-   Collaborative data science environment.

-   Notebooks for performing data science (e.g., coding, markup, etc.)

-   Tight control over data and products.

-   Flexibility to work with new and emerging technologies.

-   A lot of compute power.

-   Ability to share data and products with clients.

### Executive

The Executive is responsible for providing leadership and setting the direction for the organization. Responsibilities may be developing and executing on strategy, planning or delivery of programs or portfolios.

They use business intelligence tools for evidence-based decisions, performance monitoring and planning (e.g., HR, financial).

#### Requirements

-   Rapid insights into performance.

-   Ability to look at and combine data from across the organization
    based on the business lens.

-   Data that can be trusted for evidence-based decision making.

-   Visually appealing reports and dashboards.

-   Integrated tools.

-   Self-serve business intelligence and occasionally analytics tools (e.g., forecasting).

### NRCAN Client

The NRCAN Client collaborates with the organization and consumes available services. Often working closely with a specific department or team, they share information (e.g., data, publications), make use of available tools and insights, and deliver solutions in collaboration with NRCAN to achieve a business goal of their public/private organization.

#### Requirements

-   Ability to easily and securely share data, models and documents with
    NRCAN.

-   Ability to consume NRCAN services without risk of information being leaked to competitors.

-   Data that is well documented and easy to discover.

-   Flexible and open tools for business intelligence and analytics provided by NRCAN.


### DataHub Administrator

The DataHub Administrator is responsible for provide support and administrative services for DataHub clients. Working primarily with high-level tools, they are very familiar with the platform, monitor its performance, and ensure that the platform continues to function as
 expected. While they may use the underlying infrastructure on occasion, most of their work is focused on 1st or 2nd line support issues.

#### Requirements

-   Simple to use environment.

-   Tools that are conducive to solving client issues.

-   Ability to log issues and track resolution.

-   Ability to solve problems without programming.

-   Simple solutions that automate repetitive, high risk and/or complex
    tasks.


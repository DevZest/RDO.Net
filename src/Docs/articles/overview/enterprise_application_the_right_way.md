---
uid: enterprise_application_the_right_way
---

# Enterprise Application, the Right Way

## Buy vs. Build Dilemma

[[1]] Today, more and more managers face the task of implementing software solutions to solve operational challenges involving quality, business process improvement, compliance, data management, customer service and more. Typically, the choice boils down to selecting between off-the-shelf packaged software systems or developing custom software. It is not an easy decision, and it is one the company will live with for years to come.

[[2]] Let’s step back a moment to look at the issues involved. Consider the following (simplified) diagram:

![image](/images/BuyVsBuild.jpg)

As shown above, this low-to-high customization axis usually correlates directly to three other aspects: cost, suitability, and time to deployment. If you truly have a choice there’s a very useful rule of thumb that’s based on your business situation:

* Buy if the system is a fundamental requirement of doing business.
* Build if the system will give you an advantage over your competitors.

## The World and Business is Changing

[[3]] Virtually every industry has been experiencing rapid, massive, and sometimes devastating change over the last couple years.

Just look at what Airbnb has done to the hospitality industry. Or what Uber and Lyft have done to the transportation industry. How Spotify prompted Apple Music to advance their iTunes platform—which was itself a profound innovation to the music industry.

This requires enterprise applications to be flexible, which means you can pivot when needed, and you can adapt on the fly to this fast changing world. Competitiveness is getting more and more important over the fundamental requirement of doing business - it becomes a matter of live or die. In many cases, you are out of the "Buy" choice in the "Buy vs. Build Dilemma".

## Technology Innovation is the Rescue

We have more and more requirements to build custom applications to support our business. We need to move our enterprise applications from heavyweight to lightweight and agile, with greatly reduced cost and time to deployment. Enterprise applications should be:

* Rapid development - direct cost and time to deployment
* Fully testable - quality and
* Confident to massive changes - business flexibility (suitability)

These are the design goals of RDO.Net.

## The Challenges

For decades, the following challenges exist in enterprise application development, or more broadly speaking, in computer science:

* [Object-Relational Mapping (ORM, O/RM, and O/R mapping tool)](https://en.wikipedia.org/wiki/Object-relational_mapping), is still [The Vietnam of Computer Science](http://blogs.tedneward.com/post/the-vietnam-of-computer-science/). Particularly, these difficulties are referred to as the [object-relational impedance mismatch](https://en.wikipedia.org/wiki/Object-relational_impedance_mismatch).
* Database testing, still stays on principles and guidelines. No widely practical use yet. Refactoring an enterprise
application is error prone due to lack of database testing.
* Separation of the graphical user interface from the business logic or back-end logic (the data model), is still a challenge task. Frameworks such as [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) exists, but it's far from ideal: it will hit the wall when dealing with complex layout or complex interactivity; refactoring UI logic is still error prone, etc.

These are the problems that RDO.Net gonna to solve.

[1]: https://espressomoon.com/packaged-or-custom-software/
[2]: http://www.baselinemag.com/c/a/Application-Development/Buy-vs-Build-Software-Applications-The-Eternal-Dilemma/1
[3]: https://www.forbes.com/sites/quora/2018/01/05/how-can-businesses-adapt-to-a-rapidly-changing-world/#57b42a925930
